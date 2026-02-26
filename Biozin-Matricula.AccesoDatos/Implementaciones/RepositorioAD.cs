using System.Linq.Expressions;
using Biozin_Matricula.Dominio.InterfacesAD;
using Biozin_Matricula.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace Biozin_Matricula.AccesoDatos.Implementaciones
{
    public class RepositorioAD<T> : IRepositorioAD<T> where T : class
    {
        private readonly DbContext _contexto;
        private readonly DbSet<T> _dbSet;

        public RepositorioAD(DbContext contexto)
        {
            _contexto = contexto;
            _dbSet = contexto.Set<T>();
        }

        public Respuesta<T> ObtenerEntidad(Expression<Func<T, bool>> filtro)
        {
            var resultado = new Respuesta<T>();
            resultado.ValorRetorno = _dbSet.FirstOrDefault(filtro);
            return resultado;
        }

        public Respuesta<IEnumerable<T>> ObtenerEntidades(Expression<Func<T, bool>> filtro)
        {
            var resultado = new Respuesta<IEnumerable<T>>();
            resultado.ValorRetorno = _dbSet.Where(filtro).ToList();
            return resultado;
        }

        public Respuesta<IEnumerable<T>> Listar()
        {
            var resultado = new Respuesta<IEnumerable<T>>();
            resultado.ValorRetorno = _dbSet.ToList();
            return resultado;
        }

        public void Insertar(T entidad)
        {
            _dbSet.Add(entidad);
        }

        public void Modificar(T entidad)
        {
            _dbSet.Update(entidad);
        }

        public void Eliminar(T entidad)
        {
            _dbSet.Remove(entidad);
        }
    }
}
