using OnlineCompiler.Shared;
using Microsoft.AspNetCore.Mvc;
using OnlineCompiler.Server.Handlers;

namespace OnlineCompiler.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExecutionController : ControllerBase
    {
        private static Dictionary<string, CodeExecutor> _codeExecutors = new Dictionary<string, CodeExecutor>();
        
        private readonly ILogger<ExecutionController> _logger;

        public ExecutionController(ILogger<ExecutionController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Request for compiling and executing code
        /// </summary>
        /// <param name="code">C# code</param>
        /// <returns>Unique id of the operation</returns>
        [HttpPost]
        [Route("Dictionary")]
        public ExecutionInfo Post([FromBody]string? code)
        {
            if (code == null)
                return null;
            try
            {
                var dicType = DynamicClassCreator.CreateClassFromCode(code, "Dictionary");
                Type constructedType = dicType.MakeGenericType(typeof(string),typeof(string));
                if (constructedType != null)
                {
                    var carInstance = Activator.CreateInstance(constructedType);
                    constructedType.GetMethod("Add").Invoke(carInstance, new Object[]{"suka", "suk"});
                    constructedType.GetMethod("Add").Invoke(carInstance, new Object[]{"wtf", "wtf1"});
                }
            }
            catch (ArgumentException e)
            {
                return new ExecutionInfo(ExecutionInfo.ExecutionStatus.CompilationError, 242, e.Message);
            }
            catch (Exception ex)
            {
                return new ExecutionInfo(ExecutionInfo.ExecutionStatus.CompilationError, 0, $"Произошла ошибка при запуске: {ex}");
            }
            
            return new ExecutionInfo(ExecutionInfo.ExecutionStatus.Finished, 111, "");
        }
        
        [HttpPost]
        [Route("HashSet")]
        public ExecutionInfo PostHashSet([FromBody]string? code)
        {
            if (code == null)
            {
                return null;
            }

            try
            {
                var setType = DynamicClassCreator.CreateClassFromCode(code, "HashSet");
                Type constructedType = setType.MakeGenericType(typeof(string));
                
                if (constructedType != null)
                {
                    var setInstance = Activator.CreateInstance(constructedType);
                    constructedType.GetMethod("Add").Invoke(setInstance, new Object[]{"firstElement"});
                    constructedType.GetMethod("Add").Invoke(setInstance, new Object[]{"firstElement"});
                    // В HashSet допустимы только уникальные элементы, 
                    // поэтому, несмотря на два вызова Add с "firstElement", 
                    // размер HashSet должен быть равен 1.
                    if ((int)constructedType.GetProperty("Count").GetValue(setInstance) != 1)
                    {
                        throw new Exception($"Ошибка: ожидался размер 1, но получен размер {constructedType.GetProperty("Count").GetValue(setInstance)}");
                    }
                }
            }
            catch (ArgumentException e)
            {
                return new ExecutionInfo(ExecutionInfo.ExecutionStatus.CompilationError, 242, e.Message);
            }
            catch (Exception ex)
            {
                return new ExecutionInfo(ExecutionInfo.ExecutionStatus.CompilationError, 0, $"Произошла ошибка при запуске: {ex}");
            }
            
            return new ExecutionInfo(ExecutionInfo.ExecutionStatus.Finished, 111, "");
        }


        [HttpPost]
        [Route("List")]
        public ExecutionInfo PostList([FromBody]string? code)
        {
            if (code == null)
            {
                return null;
            }

            try
            {
                var listType = DynamicClassCreator.CreateClassFromCode(code, "List");
                Type constructedType = listType.MakeGenericType(typeof(string));
                
                if (constructedType != null)
                {
                    var listInstance = Activator.CreateInstance(constructedType);
                    constructedType.GetMethod("Add").Invoke(listInstance, new Object[]{"firstElement"});
                    constructedType.GetMethod("Add").Invoke(listInstance, new Object[]{"secondElement"});
                }
            }
            catch (ArgumentException e)
            {
                return new ExecutionInfo(ExecutionInfo.ExecutionStatus.CompilationError, 242, e.Message);
            }
            catch (Exception ex)
            {
                return new ExecutionInfo(ExecutionInfo.ExecutionStatus.CompilationError, 0, $"Произошла ошибка при запуске: {ex}");
            }
            
            return new ExecutionInfo(ExecutionInfo.ExecutionStatus.Finished, 111, "");
        }


        [HttpPost]
        [Route("Queue")]
        public ExecutionInfo PostQueue([FromBody]string? code)
        {
            if (code == null)
            {
                return null;
            }

            try
            {
                var queueType = DynamicClassCreator.CreateClassFromCode(code, "Queue");
                Type constructedType = queueType.MakeGenericType(typeof(string));
                
                if (constructedType != null)
                {
                    var queueInstance = Activator.CreateInstance(constructedType);
                    constructedType.GetMethod("Enqueue").Invoke(queueInstance, new Object[]{"firstElement"});
                    constructedType.GetMethod("Enqueue").Invoke(queueInstance, new Object[]{"secondElement"});
                    
                    // Первый добавленный элемент должен быть первым удаленным в очереди.
                    var firstElement = constructedType.GetMethod("Dequeue").Invoke(queueInstance, null);
                    if (!"firstElement".Equals(firstElement))
                    {
                        throw new Exception($"Ошибка: ожидался элемент 'firstElement', но получен {firstElement}");
                    }
                }
            }
            catch (ArgumentException e)
            {
                return new ExecutionInfo(ExecutionInfo.ExecutionStatus.CompilationError, 242, e.Message);
            }
            catch (Exception ex)
            {
                return new ExecutionInfo(ExecutionInfo.ExecutionStatus.CompilationError, 0, $"Произошла ошибка при запуске: {ex}");
            }
            
            return new ExecutionInfo(ExecutionInfo.ExecutionStatus.Finished, 111, "");
        }


        [HttpPost]
        [Route("Stack")]
        public ExecutionInfo PostStack([FromBody]string? code)
        {
            if (code == null)
            {
                return null;
            }

            try
            {
                var stackType = DynamicClassCreator.CreateClassFromCode(code, "Stack");
                Type constructedType = stackType.MakeGenericType(typeof(string));
                
                if (constructedType != null)
                {
                    var stackInstance = Activator.CreateInstance(constructedType);
                    constructedType.GetMethod("Push").Invoke(stackInstance, new Object[]{"firstElement"});
                    constructedType.GetMethod("Push").Invoke(stackInstance, new Object[]{"secondElement"});
                    
                    // Последний добавленный элемент должен быть первым удаленным в стеке.
                    var lastElement = constructedType.GetMethod("Pop").Invoke(stackInstance, null);
                    if (!"secondElement".Equals(lastElement))
                    {
                        throw new Exception($"Ошибка: ожидался элемент 'secondElement', но получен {lastElement}");
                    }

                    var firstElement = constructedType.GetMethod("Pop").Invoke(stackInstance, null);
                    if (!"firstElement".Equals(firstElement))
                    {
                        throw new Exception($"Ошибка: ожидался элемент 'firstElement', но получен {firstElement}");
                    }
                }
            }
            catch (ArgumentException e)
            {
                return new ExecutionInfo(ExecutionInfo.ExecutionStatus.CompilationError, 242, e.Message);
            }
            catch (Exception ex)
            {
                return new ExecutionInfo(ExecutionInfo.ExecutionStatus.CompilationError, 0, $"Произошла ошибка при запуске: {ex}");
            }
            
            return new ExecutionInfo(ExecutionInfo.ExecutionStatus.Finished, 111, "");
        }

        [HttpPost]
        [Route("SortedList")]
        public ExecutionInfo PostSortedList([FromBody]string? code)
        {
            if (code == null)
            {
                return null;
            }

            try
            {
                var sortedListType = DynamicClassCreator.CreateClassFromCode(code, "SortedList");
                Type constructedType = sortedListType.MakeGenericType(typeof(string), typeof(string));

                if (constructedType != null)
                {
                    var sortedListInstance = Activator.CreateInstance(constructedType);

                    // Add elements
                    constructedType.GetMethod("Add").Invoke(sortedListInstance, new Object[]{"firstKey", "firstValue"});
                    constructedType.GetMethod("Add").Invoke(sortedListInstance, new Object[]{"secondKey", "secondValue"});
                    
                    // Remove an element
                    var removeResult = constructedType.GetMethod("Remove").Invoke(sortedListInstance, new Object[]{"firstKey"});
                    
                    if ((bool)removeResult == false)
                    {
                        throw new Exception("Error: Unable to remove an element.");
                    }
                    
                    // Get value
                    var getValueResult = constructedType.GetMethod("GetValueOrDefault").Invoke(sortedListInstance, new Object[]{"secondKey"});
                    
                    if ((string)getValueResult != "secondValue")
                    {
                        throw new Exception($"Error: The value returned was not correct. Expected: 'secondValue', Actual: '{getValueResult}'");
                    }
                }
            }
            catch (ArgumentException e)
            {
                return new ExecutionInfo(ExecutionInfo.ExecutionStatus.CompilationError, 242, e.Message);
            }
            catch (Exception ex)
            {
                return new ExecutionInfo(ExecutionInfo.ExecutionStatus.CompilationError, 0, $"Произошла ошибка при запуске: {ex}");
            }
            
            return new ExecutionInfo(ExecutionInfo.ExecutionStatus.Finished, 111, "");
        }

        
      
        [HttpPost]
        [Route("ObservableCollection")]
        public ExecutionInfo PostObservableCollection([FromBody]string? code)
        {
            if (code == null)
            {
                return null;
            }

            try
            {
                var observableCollectionType = DynamicClassCreator.CreateClassFromCode(code, "ObservableCollection");
                Type constructedType = observableCollectionType.MakeGenericType(typeof(string));

                if (constructedType != null)
                {
                    var observableCollectionInstance = Activator.CreateInstance(constructedType);

                    // Add element
                    constructedType.GetMethod("Add").Invoke(observableCollectionInstance, new Object[]{"firstValue"});
                    
                    // Remove an element
                    var removeResult = constructedType.GetMethod("Remove").Invoke(observableCollectionInstance, new Object[]{"firstValue"});
                    
                    if ((bool)removeResult == false)
                    {
                        throw new Exception("Error: Unable to remove an element.");
                    }
                    
                    // Check count property
                    var countProperty = constructedType.GetProperty("Count");
                    if ((int)countProperty.GetValue(observableCollectionInstance) != 0)
                    {
                        throw new Exception($"Error: The count property returned incorrect value. Expected: 0, Actual: {(int)countProperty.GetValue(observableCollectionInstance)}");
                    }
                }
            }
            catch (ArgumentException e)
            {
                return new ExecutionInfo(ExecutionInfo.ExecutionStatus.CompilationError, 242, e.Message);
            }
            catch (Exception ex)
            {
                return new ExecutionInfo(ExecutionInfo.ExecutionStatus.CompilationError, 0, $"Произошла ошибка при запуске: {ex}");
            }
            
            return new ExecutionInfo(ExecutionInfo.ExecutionStatus.Finished, 111, "");
        }


        [HttpPost]
        [Route("LinkedList")]
        public ExecutionInfo PostLinkedList([FromBody]string? code)
        {
            if (code == null)
            {
                return null;
            }

            try
            {
                var listType = DynamicClassCreator.CreateClassFromCode(code, "LinkedList");
                Type constructedType = listType.MakeGenericType(typeof(string));
                
                if (constructedType != null)
                {
                    var listInstance = Activator.CreateInstance(constructedType);
                    
                    constructedType.GetMethod("Add").Invoke(listInstance, new Object[]{"firstElement"});
                    constructedType.GetMethod("Add").Invoke(listInstance, new Object[]{"secondElement"});
                    
                    // Проверяем подсчёт элементов.
                    var count = (int)constructedType.GetProperty("Count").GetValue(listInstance);
                    if (count != 2)
                    {
                        throw new Exception($"Ошибка: ожидалось 2 элемента, но получено {count}");
                    }
                    
                    // Удаляем элемент и проверяем подсчёт снова.
                    constructedType.GetMethod("Remove").Invoke(listInstance, new Object[]{"secondElement"});
                    count = (int)constructedType.GetProperty("Count").GetValue(listInstance);
                    if (count != 1)
                    {
                        throw new Exception($"Ошибка: ожидался 1 элемент, но получено {count}");
                    }
                }
            }
            catch (ArgumentException e)
            {
                return new ExecutionInfo(ExecutionInfo.ExecutionStatus.CompilationError, 242, e.Message);
            }
            catch (Exception ex)
            {
                return new ExecutionInfo(ExecutionInfo.ExecutionStatus.CompilationError, 0, $"Произошла ошибка при запуске: {ex}");
            }
            
            return new ExecutionInfo(ExecutionInfo.ExecutionStatus.Finished, 111, "");
        }

        /// <summary>
        /// Getting execution information by id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>Execution Information</returns>
        [HttpGet]
        [Route("{id}")]
        public ExecutionInfo? Get(string id)
        {
            if (!_codeExecutors.ContainsKey(id))
                return null;
            return _codeExecutors[id].ExecutionInfo;
        }
    }
}