using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Crossoverse.Domain.Content
{
    public interface IContentHeaderRepository
    {
        UniTask<ContentHeaderEntity> FindByIdAsync(string id);
    }
}