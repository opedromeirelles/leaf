﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Leaf.Data;
using Leaf.Models.Domain;

namespace Leaf.Repository.Agentes
{
    public class PessoaRepository : baseSqlComandos
    {
        private readonly DbConnectionManager _dbConnectionManager;

        public PessoaRepository(DbConnectionManager dbConnectionManager)
        {
            _dbConnectionManager = dbConnectionManager;
        }

        // MÉTODO PARA MAPEAR PESSOA
        public Pessoa MapearPessoa(SqlDataReader reader)
        {
			return new Pessoa
			{
				IdPessoa = reader["idpessoa"] != DBNull.Value ? Convert.ToInt32(reader["idpessoa"]) : 0, // Assumindo 0 como valor padrão
				Nome = reader["nome"] != DBNull.Value ? reader["nome"].ToString() : string.Empty,
				Tipo = reader["tipo"] != DBNull.Value ? reader["tipo"].ToString() : string.Empty,
				Cpf = reader["cpf"] != DBNull.Value ? reader["cpf"].ToString() : string.Empty,
				Cnpj = reader["cnpj"] != DBNull.Value ? reader["cnpj"].ToString() : string.Empty,
				Telefone1 = reader["telefone1"] != DBNull.Value ? reader["telefone1"].ToString() : string.Empty,
				Telefone2 = reader["telefone2"] != DBNull.Value ? reader["telefone2"].ToString() : string.Empty,
				Email1 = reader["email1"] != DBNull.Value ? reader["email1"].ToString() : string.Empty,
				Email2 = reader["email2"] != DBNull.Value ? reader["email2"].ToString() : string.Empty
			};
		}

        // MÉTODOS DE BUSCA
        public List<Pessoa> GetPessoas()
        {
            using (SqlConnection conn = _dbConnectionManager.GetConnection())
            {
                string sql = @"SELECT * FROM pessoa";
                List<Pessoa> pessoas = new List<Pessoa>();

                try
                {
                    SqlCommand command = new SqlCommand(sql, conn);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        pessoas.Add(MapearPessoa(reader));
                    }
                    return pessoas;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public List<Pessoa> GetPessoasFiltro(string nome, string tipo)
        {
            List<Pessoa> pessoas = new List<Pessoa>();

            // Base da query SQL
            string sql = @"SELECT * FROM pessoa WHERE 1 = 1";

            // Parâmetros para a consulta
            List<SqlParameter> parametros = new List<SqlParameter>();

            // Condição para nome se foi fornecido
            if (!string.IsNullOrEmpty(nome))
            {
                sql += " AND nome LIKE @nome";
                parametros.Add(new SqlParameter("@nome", "%" + nome + "%"));
            }

            // Condição para tipo se foi fornecido
            if (!string.IsNullOrEmpty(tipo))
            {
                sql += " AND tipo = @tipo";
                parametros.Add(new SqlParameter("@tipo", tipo));
            }

            // Executar a query
            using (SqlConnection conn = _dbConnectionManager.GetConnection())
            {
                SqlCommand command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(parametros.ToArray());

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    pessoas.Add(MapearPessoa(reader));
                }
            }

            return pessoas;
        }

        public Pessoa GetPessoaComCnpj(string cnpj)
        {
            string sql = @"SELECT * from pessoa where cnpj = @cnpj";


            using (SqlConnection conn = _dbConnectionManager.GetConnection())
            {
                SqlCommand command = new SqlCommand(sql, conn);
                command.Parameters.AddWithValue("@cnpj", cnpj);

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return MapearPessoa(reader);
                }
                else
                {
                    return null;
                }

            }
        }

        public async Task<List<Pessoa>> GetFornecedoresAsync(int idFornecedor)
        {
            List<Pessoa> fornecedores = new List<Pessoa>();

            string sql = @" SELECT DISTINCT  p.idpessoa, p.nome, p.tipo, p.cpf, p.cnpj, p.telefone1, p.telefone2, p.email1, p.email2
                            from PESSOA p inner join INSUMO i
                            on i.id_pessoa = p.idpessoa
                            where 1=1";            

            using (SqlConnection conn = _dbConnectionManager.GetConnection())
            {
                SqlCommand command = new SqlCommand(sql, conn);

                if (idFornecedor != 0)
                {
                    sql += @" and i.id_pessoa = @idFornecedor";
                    command.Parameters.AddWithValue("@idFornecedor", idFornecedor);
                }

                command.CommandText = sql;

                try
                {
                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        fornecedores.Add(MapearPessoa(reader));
                    }

                    return fornecedores.Any() ? fornecedores : new List<Pessoa>();
                }
                catch (SqlException ex)
                {

                    throw new Exception("Não foi possivel trazer a lista de fornecedores, erro: " + ex.Message);
                }
            }
        }

        public Pessoa GetPessoaCnpjNome(string dadosPessoa)
        {
            string sql = @"SELECT * from pessoa where 1=1";

            using (SqlConnection conn = _dbConnectionManager.GetConnection())
            {
                SqlCommand command = new SqlCommand(sql, conn);

                if (!string.IsNullOrEmpty(dadosPessoa))
                {
                    sql += "AND cnpj LIKE @cnpj";
                    command.Parameters.AddWithValue("@cnpj", dadosPessoa);
                }
                if (!string.IsNullOrEmpty(dadosPessoa))
                {
                    sql += " AND nome LIKE @nome AND cnpj IS NOT NULL";
                    command.Parameters.AddWithValue("@nome", dadosPessoa);
                }
                if (string.IsNullOrEmpty(dadosPessoa))
                {
                    return null;
                }

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return MapearPessoa(reader);
                }
                else
                {
                    return null;
                }

            }
        }

        public Pessoa GetPessoaById(int idPessoa)
        {
            using (SqlConnection conn = _dbConnectionManager.GetConnection())
            {
                string sql = @"SELECT * FROM pessoa WHERE idpessoa = @idpessoa";

                SqlCommand command = new SqlCommand(sql, conn);
                command.Parameters.AddWithValue("@idpessoa", idPessoa);

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return MapearPessoa(reader);
                }
            }
            return null;
        }

        public List<Pessoa> GetFornecedores()
        {
            string sql = @"select distinct p.idpessoa, p.nome, p.tipo, p.cpf, p.cnpj, p.telefone1, p.telefone2, p.email1, p.email2
                           from pessoa p
                           inner join insumo i
                           on p.idpessoa = i.id_pessoa;";

            List<Pessoa> fornecedores = new List<Pessoa>();

            using (SqlConnection conn = _dbConnectionManager.GetConnection())
            {
                SqlCommand command = new SqlCommand(sql, conn);
                try
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        fornecedores.Add(MapearPessoa(reader));
                    }

                    return fornecedores ?? new List<Pessoa>();
                }
                catch (SqlException ex)
                {
                    throw new Exception("Não foi possivel acessar os dados solicitados, erro: " + ex.Message);
                }

            }

        }



        // MÉTODOS DE AÇÃO

        public void CadastrarPessoa(Pessoa pessoa)
        {
            using (SqlConnection conn = _dbConnectionManager.GetConnection())
            {
                try
                {
                    string sql = @"INSERT INTO pessoa
                                    (nome, tipo, cpf, cnpj, telefone1, telefone2, email1, email2)
                                    VALUES (@nome, @tipo, @cpf, @cnpj, @telefone1, @telefone2, @email1, @email2)";

                    List<SqlParameter> parametros = new List<SqlParameter>
                    {
                        new SqlParameter("@nome", pessoa.Nome),
                        new SqlParameter("@tipo", pessoa.Tipo),
                        new SqlParameter("@cpf", pessoa.Cpf ?? (object)DBNull.Value),
                        new SqlParameter("@cnpj", pessoa.Cnpj ?? (object)DBNull.Value),
                        new SqlParameter("@telefone1", pessoa.Telefone1 ?? (object)DBNull.Value),
                        new SqlParameter("@telefone2", pessoa.Telefone2 ?? (object)DBNull.Value),
                        new SqlParameter("@email1", pessoa.Email1 ?? (object)DBNull.Value),
                        new SqlParameter("@email2", pessoa.Email2 ?? (object)DBNull.Value)
                    };

                    executaSql(sql, parametros, conn);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao cadastrar pessoa, erro: {ex.Message}");
                }
            }
        }

        public void AtualizarPessoa(Pessoa pessoa)
        {
            using (SqlConnection conn = _dbConnectionManager.GetConnection())
            {
                string sql = @"UPDATE pessoa SET
                                nome = @nome,
                                tipo = @tipo,
                                cpf = @cpf,
                                cnpj = @cnpj,
                                telefone1 = @telefone1,
                                telefone2 = @telefone2,
                                email1 = @email1,
                                email2 = @email2
                                WHERE idpessoa = @idpessoa";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@idpessoa", pessoa.IdPessoa),
                    new SqlParameter("@nome", pessoa.Nome),
                    new SqlParameter("@tipo", pessoa.Tipo),
                    new SqlParameter("@cpf", pessoa.Cpf ?? (object)DBNull.Value),
                    new SqlParameter("@cnpj", pessoa.Cnpj ?? (object)DBNull.Value),
                    new SqlParameter("@telefone1", pessoa.Telefone1 ?? (object)DBNull.Value),
                    new SqlParameter("@telefone2", pessoa.Telefone2 ?? (object)DBNull.Value),
                    new SqlParameter("@email1", pessoa.Email1 ?? (object)DBNull.Value),
                    new SqlParameter("@email2", pessoa.Email2 ?? (object)DBNull.Value)
                };

                try
                {
                    executaSql(sql, parametros, conn);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao atualizar pessoa, erro: {ex.Message}");
                }
            }
        }

        public void AtualizarStatusPessoa(int idPessoa, string tipo)
        {
            using (SqlConnection conn = _dbConnectionManager.GetConnection())
            {
                string sql = @"UPDATE pessoa SET tipo = @tipo WHERE idpessoa = @idpessoa";

                SqlCommand command = new SqlCommand(sql, conn);
                command.Parameters.AddWithValue("@idpessoa", idPessoa);
                command.Parameters.AddWithValue("@tipo", tipo);

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao alterar o status da pessoa, erro: {ex.Message}");
                }
            }
        }
    }
}
