using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using TodoApi.Database;
using TodoApi.Models;
using Microsoft.Identity.Web.Resource;

namespace TodoApi.Controllers
{
    [Authorize] // Only authorized users can access
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
        // [RequiredScope(new string[] { "Items.Read" })] // Throws a 500?
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TodoItemDTO>>> GetTodoItems()
        {
            _logger.LogInformation("before check");
            // Need this when roles are mixed with scopes.
            // User object can be explored with claims/Identity to get info on user
            if (!HttpContext.User.IsInRole("read_as_application"))
            {
                HttpContext.VerifyUserHasAnyAcceptedScope(new string[] { "Items.Read" }); // non attribute way of verifying scopes
            }
            _logger.LogInformation("{@Name} calls Get", HttpContext.User.Identity.Name ?? "Daemon App");
            var todoItems =  await _database.ListAsync();
            var itemsList = todoItems.Select(x => ItemToDTO(x)).ToList();
            return itemsList;
        }

        /// <summary>
        /// Gets a specific item by its id.
        /// </summary>
        /// <param name="id">The id of the TodoItem</param>
        /// <returns>The specific TodoItem</returns>
        // [RequiredScope(new string[] { "Items.Read" })]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TodoItemDTO>> GetTodoItem(int id)
        {
            if (!HttpContext.User.IsInRole("read_as_application"))
            {
                HttpContext.VerifyUserHasAnyAcceptedScope(new string[] { "Items.Read" }); // non attribute way of verifying scopes
            }

            using(LogContext.PushProperty("TestKey","testValue")) {

                _logger.LogInformation("{@Name} calls Get {@Id}", HttpContext.User.Identity.Name ?? "Daemon App", id);
                var todoItem = await _database.GetAsync(id);

                if (todoItem == null)
                {
                    _logger.LogInformation("No Todo Item with id '{@Id}' found", id);
                    return NotFound();
                }

                return ItemToDTO(todoItem);
            }
        }

        /// <summary>
        /// Updates a TodoItem by its id.
        /// </summary>
        /// <param name="id">The id of the TodoItem being updated</param>
        /// <param name="todoItemDTO">The deserialized updated item from the request</param>
        /// <returns>No content</returns>
        // [RequiredScope(new string[] { "Items.Write" })]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateTodoItem(int id, TodoItemDTO todoItemDTO)
        {
            if (!HttpContext.User.IsInRole("write_as_application"))
            {
                HttpContext.VerifyUserHasAnyAcceptedScope(new string[] { "Items.Write" }); // non attribute way of verifying scopes
            }

            _logger.LogInformation("{@Name} calls Put {@Id}", HttpContext.User.Identity.Name ?? "Daemon App", id);
            if (id != todoItemDTO.Id)
            {
                _logger.LogError("Provided id '{@Id}' does not match id of input Todo Item object", id);
                return BadRequest();
            }

            var todoItem = await _database.GetAsync(id);
            if (todoItem == null)
            {
                _logger.LogError("No Todo Item with id '{@Id}' found", id);
                return NotFound();
            }

            todoItem.Name = todoItemDTO.Name;
            todoItem.IsComplete = todoItemDTO.IsComplete;

            try {
                await _database.UpdateAsync(todoItem);
            } catch (ArgumentException e) {
                _logger.LogError("Error Encountered: {@Message}", e.Message);
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Creates a new TodoItem.
        /// </summary>
        /// <param name="todoItemDTO">The deserialized TodoItem from the request</param>
        /// <returns>The Created TodoItem</returns>
        // [RequiredScope(new string[] { "Items.Write" })]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<TodoItemDTO>> CreateTodoItem(TodoItemDTO todoItemDTO)
        {
            _logger.LogInformation("before check");
            if (!HttpContext.User.IsInRole("write_as_application"))
            {
                _logger.LogInformation("in check");
                HttpContext.VerifyUserHasAnyAcceptedScope(new string[] { "Items.Write" }); // non attribute way of verifying scopes
                _logger.LogInformation("after check");
            }
            _logger.LogInformation("{@Name} calls Post", HttpContext.User.Identity.Name ?? "Daemon App");
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
        // [RequiredScope(new string[] { "Items.Write" })]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteTodoItem(int id)
        {
            if (!HttpContext.User.IsInRole("write_as_application"))
            {
                HttpContext.VerifyUserHasAnyAcceptedScope(new string[] { "Items.Write" }); // non attribute way of verifying scopes
            }

            _logger.LogInformation("{@Name} calls Delete {@Id}", HttpContext.User.Identity.Name ?? "Daemon App", id);
            var todoItem = await _database.GetAsync(id);

            if (todoItem == null)
            {
                _logger.LogError("No Todo Item with id '{@Id}' found", id);
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
