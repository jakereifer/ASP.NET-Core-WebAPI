using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TodoApi.Database;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    public class TodoItemsController : ControllerBase
    {
        private readonly IMockDB<TodoItem> _database;
        private readonly ILogger<TodoItemsController> _logger;

        public TodoItemsController(
            IMockDB<TodoItem> database,
            ILogger<TodoItemsController> logger
        )
        {
            _database = database;
            _logger = logger;
        }

        /// <summary>
        /// Gets all items.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TodoItemDTO>>> GetTodoItems()
        {
            _logger.LogInformation("'GET' called");
            var todoItems =  await _database.ListAsync();
            var itemsList = todoItems.Select(x => ItemToDTO(x)).ToList();
            return itemsList;
        }

        /// <summary>
        /// Gets a specific item by its id.
        /// </summary>
        /// <param name="id">The id of the TodoItem</param>
        /// <returns>The specific TodoItem</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TodoItemDTO>> GetTodoItem(int id)
        {
            _logger.LogInformation($"'GET {id}' called");
            var todoItem = await _database.GetAsync(id);

            if (todoItem == null)
            {
                _logger.LogError($"No Todo Item with id '{id}' found");
                return NotFound();
            }

            return ItemToDTO(todoItem);
        }

        /// <summary>
        /// Updates a TodoItem by its id.
        /// </summary>
        /// <param name="id">The id of the TodoItem being updated</param>
        /// <param name="todoItemDTO">The deserialized updated item from the request</param>
        /// <returns>No content</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateTodoItem(int id, TodoItemDTO todoItemDTO)
        {
            _logger.LogInformation($"'PUT {id}' called");
            if (id != todoItemDTO.Id)
            {
                _logger.LogError($"Provided id '{id}' does not match id of input Todo Item object");
                return BadRequest();
            }

            var todoItem = await _database.GetAsync(id);
            if (todoItem == null)
            {
                _logger.LogError($"No Todo Item with id '{id}' found");
                return NotFound();
            }

            todoItem.Name = todoItemDTO.Name;
            todoItem.IsComplete = todoItemDTO.IsComplete;

            try {
                await _database.UpdateAsync(todoItem);
            } catch (ArgumentException e) {
                _logger.LogError($"Error Encountered: {e.Message}");
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Creates a new TodoItem.
        /// </summary>
        /// <param name="todoItemDTO">The deserialized TodoItem from the request</param>
        /// <returns>The Created TodoItem</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<TodoItemDTO>> CreateTodoItem(TodoItemDTO todoItemDTO)
        {
            _logger.LogInformation($"'POST' called");
            var todoItem = new TodoItem
            {
                IsComplete = todoItemDTO.IsComplete,
                Name = todoItemDTO.Name
            };

            await _database.AddAsync(todoItem);

            return CreatedAtAction(
                nameof(GetTodoItem),
                new { id = todoItem.Id },
                ItemToDTO(todoItem));
        }

        /// <summary>
        /// Deletes a specific TodoItem.
        /// </summary>
        /// <param name="id">The id of the TodoItem to be deleted</param>
        /// <returns>No Content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteTodoItem(int id)
        {
            _logger.LogInformation($"'DELETE {id}' called");
            var todoItem = await _database.GetAsync(id);

            if (todoItem == null)
            {
                _logger.LogError($"No Todo Item with id '{id}' found");
                return NotFound();
            }

            await _database.DeleteAsync(id);

            return NoContent();
        }

        private static TodoItemDTO ItemToDTO(TodoItem todoItem) =>
            new TodoItemDTO
            {
                Id = todoItem.Id,
                Name = todoItem.Name,
                IsComplete = todoItem.IsComplete
            };
    }
}
