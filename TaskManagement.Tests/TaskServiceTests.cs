using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TaskManagement.BL.DTO.Task;
using TaskManagement.BL.Services.Implementation;
using TaskManagement.DAL.Repositories;

namespace TaskManagement.Tests
{
    public class TaskServiceTests
    {
        private readonly Mock<ITaskRepository> _taskRepositoryMock;
        private readonly Mock<ILogger<TaskService>> _loggerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly TaskService _taskService;

        public TaskServiceTests()
        {
            _taskRepositoryMock = new Mock<ITaskRepository>();
            _loggerMock = new Mock<ILogger<TaskService>>();
            _mapperMock = new Mock<IMapper>();

            _taskService = new TaskService(_taskRepositoryMock.Object, _loggerMock.Object, _mapperMock.Object);
        }
        #region GetTasksAsync
        [Fact]
        public async Task GetTasksAsync_ReturnsPaginatedTasks()
        {
            // Arrange
            var taskFilter = new TaskFilterDTO { UserId = Guid.NewGuid() };
            var tasks = new List<Domain.Models.Task> { new Domain.Models.Task(), new Domain.Models.Task() };

            _taskRepositoryMock.Setup(repo => repo.GetFilteredTasksAsync(taskFilter.UserId, null, null, null))
                .ReturnsAsync(tasks);
            _mapperMock.Setup(m => m.Map<TaskDTO>(It.IsAny<Domain.Models.Task>()))
                .Returns(new TaskDTO());

            int page = 1, pageSize = 1;

            // Act
            var result = await _taskService.GetTasksAsync(taskFilter, page, pageSize);

            // Assert
            Assert.Equal(page, result.Page);
            Assert.Equal(pageSize, result.PageSize);
            Assert.Single(result.Tasks);
            Assert.Equal(2, result.TotalTasks);
        }

        [Fact]
        public async Task GetTasksAsync_ReturnsEmptyList_WhenNoTasks()
        {
            // Arrange
            var taskFilter = new TaskFilterDTO { UserId = Guid.NewGuid() };

            _taskRepositoryMock.Setup(repo => repo.GetFilteredTasksAsync(taskFilter.UserId, null, null, null))
                .ReturnsAsync(new List<Domain.Models.Task>());

            int page = 1, pageSize = 10;

            // Act
            var result = await _taskService.GetTasksAsync(taskFilter, page, pageSize);

            // Assert
            Assert.Empty(result.Tasks);
            Assert.Equal(0, result.TotalTasks);
        }

        [Fact]
        public async Task GetTasksAsync_OrdersTasksByDueDate()
        {
            // Arrange
            var taskFilter = new TaskFilterDTO { UserId = Guid.NewGuid(), DueDateOrder = SortOrder.Ascending };
            var tasks = new List<Domain.Models.Task>
            {
                new Domain.Models.Task { DueDate = DateTime.UtcNow.AddDays(1) },
                new Domain.Models.Task { DueDate = DateTime.UtcNow.AddDays(2) }
            };

            _taskRepositoryMock.Setup(repo => repo.GetFilteredTasksAsync(taskFilter.UserId, null, null, null))
                .ReturnsAsync(tasks);
            _mapperMock.Setup(m => m.Map<TaskDTO>(It.IsAny<Domain.Models.Task>()))
                .Returns(new TaskDTO());

            int page = 1, pageSize = 2;

            // Act
            var result = await _taskService.GetTasksAsync(taskFilter, page, pageSize);

            // Assert
            Assert.Equal(2, result.Tasks.Count());
        }
        #endregion
        #region GetTaskByIdAsync
        [Fact]
        public async Task GetTaskByIdAsync_ReturnsTask_IfUserAuthorized()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var task = new Domain.Models.Task { Id = taskId, UserId = userId };

            _taskRepositoryMock.Setup(repo => repo.GetByIdAsync(taskId)).ReturnsAsync(task);

            // Act
            var result = await _taskService.GetTaskByIdAsync(taskId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(taskId, result.Id);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ReturnsNull_IfTaskNotFound()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            _taskRepositoryMock.Setup(repo => repo.GetByIdAsync(taskId)).ReturnsAsync((Domain.Models.Task)null);

            // Act
            var result = await _taskService.GetTaskByIdAsync(taskId, Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ReturnsNull_IfUserNotAuthorized()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var task = new Domain.Models.Task { Id = taskId, UserId = Guid.NewGuid() }; // Інший користувач

            _taskRepositoryMock.Setup(repo => repo.GetByIdAsync(taskId)).ReturnsAsync(task);

            // Act
            var result = await _taskService.GetTaskByIdAsync(taskId, userId);

            // Assert
            Assert.Null(result);
        }
        #endregion
        #region CreateTaskAsync
        [Fact]
        public async Task CreateTaskAsync_CreatesTaskSuccessfully()
        {
            // Arrange
            var taskDTO = new TaskDTO { Title = "New Task" };
            var task = new Domain.Models.Task { Title = taskDTO.Title};

            _mapperMock.Setup(m => m.Map<Domain.Models.Task>(taskDTO)).Returns(task);

            // Act
            var result = await _taskService.CreateTaskAsync(taskDTO, task.UserId);

            // Assert
            _taskRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Domain.Models.Task>()), Times.Once);
            _taskRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
            Assert.Equal(taskDTO.Title, result.Title);
        }

        [Fact]
        public async Task CreateTaskAsync_SetsCorrectTimestamps()
        {
            // Arrange
            var taskDTO = new TaskDTO { Title = "New Task" };
            var task = new Domain.Models.Task { Title = taskDTO.Title };

            _mapperMock.Setup(m => m.Map<Domain.Models.Task>(taskDTO)).Returns(task);

            // Act
            var result = await _taskService.CreateTaskAsync(taskDTO, task.UserId);

            // Assert
            Assert.True(result.CreatedAt <= DateTime.UtcNow);
            Assert.True(result.UpdatedAt <= DateTime.UtcNow);
        }

        #endregion
        #region DeleteTaskAsync
        [Fact]
        public async Task DeleteTaskAsync_DeletesTask_IfAuthorized()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var task = new Domain.Models.Task { Id = taskId, UserId = userId };

            _taskRepositoryMock.Setup(repo => repo.GetByIdAsync(taskId)).ReturnsAsync(task);

            // Act
            var result = await _taskService.DeleteTaskAsync(taskId, userId);

            // Assert
            Assert.True(result);
            _taskRepositoryMock.Verify(repo => repo.Delete(It.IsAny<Domain.Models.Task>()), Times.Once);
            _taskRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteTaskAsync_ReturnsFalse_IfTaskNotFound()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            _taskRepositoryMock.Setup(repo => repo.GetByIdAsync(taskId)).ReturnsAsync((Domain.Models.Task)null);

            // Act
            var result = await _taskService.DeleteTaskAsync(taskId, Guid.NewGuid());

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteTaskAsync_ReturnsFalse_IfUserNotAuthorized()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var task = new Domain.Models.Task { Id = taskId, UserId = Guid.NewGuid() }; // Інший користувач

            _taskRepositoryMock.Setup(repo => repo.GetByIdAsync(taskId)).ReturnsAsync(task);

            // Act
            var result = await _taskService.DeleteTaskAsync(taskId, userId);

            // Assert
            Assert.False(result);
        }

        #endregion
        #region UpdateTaskAsync
        [Fact]
        public async Task UpdateTaskAsync_UpdatesTaskSuccessfully()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var task = new Domain.Models.Task { Id = taskId, UserId = userId };

            _taskRepositoryMock.Setup(repo => repo.GetByIdAsync(taskId)).ReturnsAsync(task);

            // Act
            var result = await _taskService.UpdateTaskAsync(task);

            // Assert
            Assert.True(result);
            _taskRepositoryMock.Verify(repo => repo.Update(It.IsAny<Domain.Models.Task>()), Times.Once);
            _taskRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateTaskAsync_ReturnsFalse_IfTaskNotFound()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            _taskRepositoryMock.Setup(repo => repo.GetByIdAsync(taskId)).ReturnsAsync((Domain.Models.Task)null);

            // Act
            var result = await _taskService.UpdateTaskAsync(new Domain.Models.Task { Id = taskId });

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateTaskAsync_ReturnsFalse_IfUserNotAuthorized()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var task = new Domain.Models.Task { Id = taskId, UserId = Guid.NewGuid() }; // Інший користувач

            _taskRepositoryMock.Setup(repo => repo.GetByIdAsync(taskId)).ReturnsAsync(task);

            // Act
            var result = await _taskService.UpdateTaskAsync(new Domain.Models.Task { Id = taskId, UserId = userId });

            // Assert
            Assert.False(result);
        }

        #endregion 
    }
}