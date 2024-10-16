﻿using Leaf.Data;
using System.Data.SqlClient;
using System.Data;
using Leaf.Models;

namespace Leaf.Repository
{
    public class LoteProducaoRepository
	{
		private readonly DbConnectionManager _dbConnectionManager;

        public LoteProducaoRepository(DbConnectionManager dbConnectionManager)
        {
			_dbConnectionManager = dbConnectionManager;

		}

		//Mapear LOTE
		public LoteProducao MapearLoteProducao(SqlDataReader reader)
		{
			return new LoteProducao
			{
				IdLote = reader["idlote"].ToString(),
				Estagio = reader["estagio"] != DBNull.Value ? Convert.ToByte(reader["estagio"]) : (byte?)null,
				IdProduto = reader["id_produto"] != DBNull.Value ? Convert.ToInt32(reader["id_produto"]) : 0,
				Produto = null, // Aqui poderá ser popular se necessário depois
				Qtde = reader["qtde"] != DBNull.Value ? Convert.ToInt32(reader["qtde"]) : 0, 
				DtaSemeadura = reader["dta_semeadura"] != DBNull.Value ? Convert.ToDateTime(reader["dta_semeadura"]) : (DateTime?)null,
				DtaCrescimento = reader["dta_crescimento"] != DBNull.Value ? Convert.ToDateTime(reader["dta_crescimento"]) : (DateTime?)null,
				DtaPlantio = reader["dta_plantio"] != DBNull.Value ? Convert.ToDateTime(reader["dta_plantio"]) : (DateTime?)null,
				DtaColheita = reader["dta_colheita"] != DBNull.Value ? Convert.ToDateTime(reader["dta_colheita"]) : (DateTime?)null,
				IdUsuario = reader["id_usuario"] != DBNull.Value ? Convert.ToInt32(reader["id_usuario"]) : 0,
				Usuario = null // Pode ser popular depois, caso seja necessário

			};
		}

		//Relatório Lote
		public List<LoteProducao> GetListLotesPeriodo(DateTime dataInicio, DateTime dataFim, int idProduto)
		{
			List<LoteProducao> lotes = new List<LoteProducao>();

			// Base da query SQL
			string sql = @"SELECT * FROM LOTE_PRODUCAO WHERE 1 = 1
                   AND dta_semeadura >= @dataInicio 
                   AND dta_semeadura <= @dataFim";

			// Lista de parâmetros para adicionar à consulta
			List<SqlParameter> parametros = new List<SqlParameter>();

			// Adiciona os parâmetros obrigatórios de datas
			parametros.Add(new SqlParameter("@dataInicio", dataInicio));
			parametros.Add(new SqlParameter("@dataFim", dataFim));

			// Condição para filtrar pelo idProduto se ele for fornecido
			if (idProduto != 0)
			{
				sql += " AND id_produto = @idProduto";
				parametros.Add(new SqlParameter("@idProduto", idProduto));
			}

			// Executa a query e faz o mapeamento dos lotes
			using (SqlConnection conn = _dbConnectionManager.GetConnection())
			{
				SqlCommand command = new SqlCommand(sql, conn);
				command.Parameters.AddRange(parametros.ToArray());

				try
				{
					SqlDataReader reader = command.ExecuteReader();

					while (reader.Read())
					{
						lotes.Add(MapearLoteProducao(reader));
					}
				}
				catch (SqlException ex)
				{
					// Tratar erro e lançar exceção, se necessário
					throw new Exception("Erro ao tentar buscar lotes de produção, erro: " + ex.Message);
				}
			}

			return lotes.Any() ? lotes : new List<LoteProducao>();
		}



	}
}
