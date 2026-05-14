using AspCoreAgent.Agent;
using EasyCore.Agent;
using EasyCore.Pipeline;
using EasyCore.Dependencie;

namespace AspCoreAgent.Tools
{
    public class PipelineTool : IScopedDependencie
    {
        private readonly DeepSeekAgent _agent;

        public PipelineTool(
            DeepSeekAgent agent)
        {
            _agent = agent;
        }

        /// <summary>
        /// 多 Agent 协同，流程执行演示
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [AITool("get_workflow_test")]
        [ToolDescription("执行流程")]
        public async Task<string?> RunAsync([ToolDescription("输入自然整数的数字，例如：1，2。数字的范围上下限是1到2。")] string input, CancellationToken cancellationToken = default)
        {
            var workflow = Pipeline.Create()
                // Step1：意图识别 Agent
                .AddFunc(Step1Async)

                // 根据 intent 选择不同流程
                .AddBranch(branch => branch

                    // intent == 1：代码生成流程
                    .If(ctx => ctx.Get<string>("intent") == "1", flow => flow
                        // Step2：计划 Agent
                        .AddFunc(Step2Async)

                        // Step3 / Step4 并行执行
                        .AddParallel(parallel => parallel
                            // Step3：Controller 生成 Agent
                            .AddFunc(Step3Async)

                            // Step4：DTO 生成 Agent
                            .AddFunc(Step4Async))

                        // Step5：合并 Agent
                        .AddFunc(Step5Async))

                    // intent == 2：SQL 生成流程
                    .ElseIf(ctx => ctx.Get<string>("intent") == "2", flow => flow
                        // Step6：SQL Agent
                        .AddFunc(Step6Async))

                    // 兜底流程
                    .Else(flow => flow
                        // Step7：普通聊天 Agent
                        .AddFunc(Step7Async)))

                // Step8：最终总结 Agent
                .AddFunc(Step8Async);

            var context = new PipelineContext
            {
                Input = input
            };

            await PipelineRunner.RunAsync(workflow, context, cancellationToken);

            return context.Output;
        }

        /// <summary>
        /// Step1：意图识别 Agent
        /// 
        /// 作用：
        /// 根据用户输入判断走哪个分支。
        /// 
        /// 输入：
        /// context.Input
        /// 
        /// 输出：
        /// context.Items["intent"]
        /// </summary>
        private async Task Step1Async(PipelineContext context, CancellationToken cancellationToken)
        {
            var input = context.Input?.Trim();

            // 模拟 IntentAgent 的判断结果
            // 实际项目里，这里可以调用 DeepSeekAgent / OpenAI Agent
            if (input == "1")
            {
                context.Set("intent", "1");
                context.Set("intent_description", "代码生成流程");
            }
            else if (input == "2")
            {
                context.Set("intent", "2");
                context.Set("intent_description", "SQL生成流程");
            }
            else
            {
                context.Set("intent", "other");
                context.Set("intent_description", "普通聊天流程");
            }

            Console.WriteLine($"step1--意图识别结果：{context.Get<string>("intent_description")}");

            await Task.CompletedTask;
        }

        /// <summary>
        /// Step2：计划 Agent
        /// 
        /// 作用：
        /// 根据用户输入生成代码生成计划。
        /// 
        /// 输入：
        /// context.Input
        /// 
        /// 输出：
        /// context.Items["plan"]
        /// context.Next(plan)
        /// </summary>
        private async Task Step2Async(PipelineContext context, CancellationToken cancellationToken)
        {
            // 模拟 PlanAgent
            var plan = $"""
                【PlanAgent 输出】
              
                用户输入：
                {context.Input}
              
                代码生成计划：
                1. 生成 ProductController
                2. 生成 ProductDto
                3. 最后合并 Controller 和 DTO
                """;

            context.Set("plan", plan);

            // Next 表示：把当前输出作为下一个步骤的输入
            context.Next(plan);

            Console.WriteLine($"step2--计划生成结果：{plan}");

            await Task.CompletedTask;
        }

        /// <summary>
        /// Step3：Controller 生成 Agent
        /// 
        /// 作用：
        /// 根据 Step2 的计划生成 Controller。
        /// 
        /// 注意：
        /// 这是并行节点，不要调用 context.Next。
        /// 并行节点只写自己的结果到 Items。
        /// 
        /// 输入：
        /// context.Items["plan"]
        /// 
        /// 输出：
        /// context.Items["controller"]
        /// </summary>
        private async Task Step3Async(PipelineContext context, CancellationToken cancellationToken)
        {
            var plan = context.Get<string>("plan");

            // 模拟 ControllerAgent
            var controller = $$"""
               【ControllerAgent 输出】
               
               根据计划生成 Controller：
               
               {{plan}}
               
               public sealed class ProductController : ControllerBase
               {
                   [HttpGet("{id}")]
                   public IActionResult Get(Guid id)
                   {
                       return Ok(new ProductDto
                       {
                           Id = id,
                           Name = "测试商品"
                       });
                   }
               }
               """;

            context.Set("controller", controller);

            Console.WriteLine($"step3--Controller 生成结果：{controller}");

            await Task.CompletedTask;
        }

        /// <summary>
        /// Step4：DTO 生成 Agent
        /// 
        /// 作用：
        /// 根据 Step2 的计划生成 DTO。
        /// 
        /// 注意：
        /// 这是并行节点，不要调用 context.Next。
        /// 并行节点只写自己的结果到 Items。
        /// 
        /// 输入：
        /// context.Items["plan"]
        /// 
        /// 输出：
        /// context.Items["dto"]
        /// </summary>
        private async Task Step4Async(PipelineContext context, CancellationToken cancellationToken)
        {
            var plan = context.Get<string>("plan");

            // 模拟 DtoAgent
            var dto = $$"""
               【DtoAgent 输出】
               
               根据计划生成 DTO：
               
               {{plan}}
               
               public sealed class ProductDto
               {
                   public Guid Id { get; set; }
               
                   public string Name { get; set; } = string.Empty;
               }
               """;

            context.Set("dto", dto);

            Console.WriteLine($"step4--DTO 生成结果：{dto}");

            await Task.CompletedTask;
        }

        /// <summary>
        /// Step5：合并 Agent
        /// 
        /// 作用：
        /// 等 Step3 和 Step4 都执行完之后，读取并行结果并合并。
        /// 
        /// 输入：
        /// context.Items["controller"]
        /// context.Items["dto"]
        /// 
        /// 输出：
        /// context.Next(result)
        /// </summary>
        private async Task Step5Async(PipelineContext context, CancellationToken cancellationToken)
        {
            var controller = context.Get<string>("controller");
            var dto = context.Get<string>("dto");

            // 模拟 MergeAgent
            var result = $"""
             【MergeAgent 输出】
          
             ===== Controller =====
          
             {controller}
          
             ===== DTO =====
          
             {dto}
          
             合并说明：
             Controller 和 DTO 已经生成完成。
             """;

            // 合并后的结果给后面的 Step8 使用
            context.Next(result);

            Console.WriteLine($"step5--合并结果：{result}");

            await Task.CompletedTask;
        }

        /// <summary>
        /// Step6：SQL 生成 Agent
        /// 
        /// 作用：
        /// 当 intent == 2 时，走 SQL 分支。
        /// 
        /// 输入：
        /// context.Input
        /// 
        /// 输出：
        /// context.Next(sqlResult)
        /// </summary>
        private async Task Step6Async(PipelineContext context, CancellationToken cancellationToken)
        {
            // 模拟 SqlAgent
            var sqlResult = $"""
               【SqlAgent 输出】
            
               用户输入：
               {context.Input}
            
               生成 SQL：
            
               SELECT *
               FROM Products
               WHERE IsDeleted = 0
               ORDER BY CreateTime DESC;
               """;

            context.Set("sql_result", sqlResult);

            context.Next(sqlResult);

            Console.WriteLine($"step6--SQL 生成结果：{sqlResult}");

            await Task.CompletedTask;
        }

        /// <summary>
        /// Step7：普通聊天 Agent
        /// 
        /// 作用：
        /// 当 intent 不是 1 或 2 时，走普通聊天分支。
        /// 
        /// 输入：
        /// context.Input
        /// 
        /// 输出：
        /// context.Next(answer)
        /// </summary>
        private async Task Step7Async(PipelineContext context, CancellationToken cancellationToken)
        {
            // 模拟 ChatAgent
            var answer = $"""
               【ChatAgent 输出】
            
               你输入的是：
               {context.Input}
            
               当前没有匹配到代码生成流程或 SQL 生成流程，所以进入普通聊天流程。
               """;

            context.Next(answer);

            Console.WriteLine($"step7--普通聊天结果：{answer}");

            await Task.CompletedTask;
        }

        /// <summary>
        /// Step8：最终总结 Agent
        /// 
        /// 作用：
        /// 所有分支执行完成后，统一做最终输出。
        /// 
        /// 输入：
        /// context.Output
        /// 
        /// 输出：
        /// context.Output
        /// </summary>
        private async Task Step8Async(PipelineContext context, CancellationToken cancellationToken)
        {
            var intent = context.Get<string>("intent");
            var intentDescription = context.Get<string>("intent_description");

            // 模拟 SummaryAgent
            context.Output = $"""
               【SummaryAgent 输出】
             
               流程执行完成。
             
               Intent：
               {intent}
             
               Intent说明：
               {intentDescription}
             
               最终结果：
             
               {context.Output}
               """;

            Console.WriteLine($"step8--最终总结结果：{context.Output}");

            await Task.CompletedTask;
        }
    }
}
