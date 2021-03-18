using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.News;
using Nop.Core.Extensions;
using Nop.Services.Events;
using System;
using System.Linq;

namespace Nop.Services.News
{
    public class NewsCategoryService : INewsCategoryService
    {
        private readonly IRepository<NewsCategory> _cateNewsRepository;
        private readonly IEventPublisher _eventPublisher;
        public NewsCategoryService(IRepository<NewsCategory> cateNewsRepository, IEventPublisher eventPublisher)
        {
            _cateNewsRepository = cateNewsRepository;
            _eventPublisher = eventPublisher;
        }

        public void DeleteNewsCategory(NewsCategory newsCate)
        {
            if (newsCate == null)
                throw new ArgumentNullException(nameof(newsCate));
            _cateNewsRepository.Delete(newsCate);
            _eventPublisher.Publish(newsCate);

        }

        public NewsCategory GetNewsCategoryById(int cateNewsId)
        {
            return _cateNewsRepository.GetById(cateNewsId);
        }

        public IPagedList<NewsCategory> GetAllNewsCategories(string searchCategoryName = null, int languageId = 0, bool showHidden = false)
        {
            var query = _cateNewsRepository.Table;
            if (showHidden)
            {
                return new PagedList<NewsCategory>(query, 0, int.MaxValue, query.Count());
            }

            if (searchCategoryName.IsNotNullOrEmpty())
            {
                query = query.Where(_ => _.Name.Contains(searchCategoryName));
            }

            if (languageId > 0)
            {
                query = query.Where(_ => _.LanguageId == languageId);
            }
            query = query.Where(_ => _.Published);
            return new PagedList<NewsCategory>(query, 0, int.MaxValue, query.Count());
        }

        public void Insert(NewsCategory newsCate)
        {
            if (newsCate == null)
                throw new ArgumentNullException(nameof(newsCate));
            _cateNewsRepository.Insert(newsCate);
        }

        public void Update(NewsCategory newsCate)
        {
            if (newsCate == null)
                throw new ArgumentNullException(nameof(newsCate));
            _cateNewsRepository.Update(newsCate);
        }
    }
}
