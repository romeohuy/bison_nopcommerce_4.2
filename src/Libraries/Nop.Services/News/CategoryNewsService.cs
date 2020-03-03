using Nop.Core.Data;
using Nop.Core.Domain.News;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Services.News
{
    public class CategoryNewsService : ICategoryNewsService
    {
        private readonly IRepository<CategoryNews> _cateNewsRepository;
        private readonly IEventPublisher _eventPublisher;
        public CategoryNewsService(IRepository<CategoryNews> cateNewsRepository, IEventPublisher eventPublisher)
        {
            _cateNewsRepository = cateNewsRepository;
            _eventPublisher = eventPublisher;
        }

        public void DeleteCategoryNews(CategoryNews cateNews)
        {
            if (cateNews == null)
                throw new ArgumentNullException(nameof(cateNews));
            _cateNewsRepository.Delete(cateNews);
            _eventPublisher.Publish(cateNews);

        }

        public CategoryNews GetCategoryNewsById(int cateNewsId)
        {
            return _cateNewsRepository.GetById(cateNewsId);
        }

        public IList<CategoryNews> GetAllCategoryNews(int languageId = 0, bool showHidden = false)
        {
            var query = _cateNewsRepository.Table;
            if (showHidden)
            {
                return query.ToList();
            }
            return query.Where(_ => _.Published).ToList();
        }

        public void InsertCategoryNews(CategoryNews cateNews)
        {
            if (cateNews == null)
                throw new ArgumentNullException(nameof(cateNews));
            _cateNewsRepository.Insert(cateNews);
        }

        public void UpdateCategoryNews(CategoryNews cateNews)
        {
            if (cateNews == null)
                throw new ArgumentNullException(nameof(cateNews));
            _cateNewsRepository.Update(cateNews);
        }
    }
}
