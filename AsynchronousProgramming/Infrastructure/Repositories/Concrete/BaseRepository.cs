using AsynchronousProgramming.Infrastructure.Context;
using AsynchronousProgramming.Infrastructure.Repositories.Interfaces;
using AsynchronousProgramming.Models.Entities.Abstract;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AsynchronousProgramming.Infrastructure.Repositories.Concrete
{
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
    {
        //Repository içerisinde neden ApplicationDbContext.cs sınıfına ihtiyacımız var ?
        private readonly ApplicationDbContext _dbContext;//ApplicationDbContext.cs sınıfı biizm uygulama tarafındaki veri tabanımızın karşılığıdır.

        protected readonly DbSet<T> _table; //DbSet ise hatırlarsanız ApplicationDbContext sınıfı içeriisnde tanımladığımız bir yapı idi. O da uygulama tarafında veri tabanında ki tabloların karşılığıdır.

        //Biz veri tabanı üzerinde herhangi bir CRUD operasyonu yapacağımız zaman teorik olarak düşünürsek veri tabanına ve onun üzerinden ilgili tabloya erişmemiz gerekmektedir. ORM gereği burada muhakkak onların bir karşılığı olmak zorundadır.
        public BaseRepository(ApplicationDbContext dbContext)
        {
            //Dependency Injection
            //Eski çalışmalarımızda tam burada yukarıda anlatığım sebpten dolayı ApplicationDbContext.cs sınıfın nesnesini çıkarırdık. Bu örneklem alma işlemi yüzünden repository sınıfları ile ApplicationDbContext.cs sınfıı arasında sıkı sıkıya bağlı bir ilişki oluşmaktaydı. Ayrıca memory yönetimi açısından sıkı sıkıya bağlı sınıfların maliyer oluşturduğunu ve RAM'in Heap alanında yönetilemeyen kaynaklara neden olmaktadır. Sonuç olarak her sınıfın instance'si çıkardığımızda bu nesnelerin yönetiminde projelerimiz büyüdükçe sıkıntılar yaşamaktayız. Bu yüzden projelerimde bu tarz bağımlılıklara sebep olan sınıfları DIP ve IoC prensiblerine de uymak için Dependency Injection desene kullanarak gevşek bağlı bir hale getirmek istiyoruz.,

            //Inject ederken 3 farklı yol ile inject edebiliriz..

            //1. Constructor Injection (şuanda kullanılan)
            //2. Custom Method Injection
            //3. Property ile Injection

            // DI bir desendir, prensip değildir. Hatta DIP ve IoC prensiplerini uygulamamızda bize yardımcı olan bir araçtır. Asp.NetCore bu prensipleri projelerimizde rahatlıkla kullanmanız için dizayn edilmiştir.
            _dbContext = dbContext;
            _table = _dbContext.Set<T>();
        }


        public async Task Add(T entity)
        {
            await _table.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> Any(Expression<Func<T, bool>> expression) => await _table.AnyAsync(expression);

        public async Task Delete(T entity) => await Update(entity);

        public async Task<T> GetByDefault(Expression<Func<T, bool>> expression) => await _table.FirstOrDefaultAsync(expression);

        public async Task<List<T>> GetByDefaults(Expression<Func<T, bool>> expression) => await _table.Where(expression).ToListAsync();

        public async Task<T> GetById(int id) => await _table.FindAsync(id);

        public async Task<List<TResult>> GetFilteredList<TResult>(Expression<Func<T, TResult>> select, 
                                                            Expression<Func<T, bool>> where = null, 
                                                            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,                                    Func<IQueryable<T>, IIncludableQueryable<T, object>> join = null)
        {
            IQueryable<T> query = _table;

            if(join != null) query = join(query);

            if(where != null) query = query.Where(where);

            if(orderBy != null)
            {
                return await orderBy(query).Select(select).ToListAsync();
            }
            else
            {
                return await query.Select(select).ToListAsync();
            }
        }

        public async Task Update(T entity)
        { 
            _dbContext.Entry<T>(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }
    }
}
