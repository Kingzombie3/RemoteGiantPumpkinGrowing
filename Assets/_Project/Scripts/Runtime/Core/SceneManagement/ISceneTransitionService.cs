using Cysharp.Threading.Tasks;

namespace _Project.Scripts.Runtime.Core.SceneManagement
{
    public interface ISceneTransitionService
    {
        UniTask LoadSceneAsync(string sceneName);
    }
}
