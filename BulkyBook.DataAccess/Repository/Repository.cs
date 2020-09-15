using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using BulkyBook.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {

        private readonly ApplicationDbContext _db;
        internal DbSet<T> DbSet;

        public Repository(ApplicationDbContext db)
        {
            _db = db;
            this.DbSet = _db.Set<T>();
        }

        public void Add(T entity)
        {
            DbSet.Add(entity);
        }

        public T Get(int id)
        {
            return DbSet.Find(id);
        }

        //public IEnumerable<T> GetAll(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = null)
        //{
        //    IQueryable<T> query = DbSet;
        //    if (filter != null)
        //    {
        //        query = query.Where(filter);
        //    }

        //    if (includeProperties != null)
        //    {
        //        foreach (var item in includeProperties.Split(new char[]{','},StringSplitOptions.RemoveEmptyEntries))
        //        {
        //            query = query.Include(item);
        //        }
        //    }

        //    if (orderBy != null)
        //    {
        //        return orderBy(query).ToList();
        //    }

        //    return query.ToList();
        //}

        public IEnumerable<T> GetAll(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = null)
        {
            IQueryable<T> query = DbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }
            return query.ToList();
        }

        public T GetFirstOrDefault(Expression<Func<T, bool>> filter = null, string IncludeProperties = null)
        {
            IQueryable<T> query = DbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (IncludeProperties != null)
            {
                foreach (var item in IncludeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(item);
                }
            }

            return query.FirstOrDefault();
        }

        public void Remove(int id)
        {
            T entity = DbSet.Find(id);
            Remove(entity);
        }

        public void Remove(T entity)
        {
            DbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
            DbSet.RemoveRange(entity);
        }
    }
}
