using System.Threading.Tasks;

namespace ManagerOfDungeon
{
    public interface ILlmClient
    {
        Task<string> Complete(LlmPrompt prompt);
    }
}
