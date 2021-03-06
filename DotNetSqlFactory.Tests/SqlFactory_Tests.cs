﻿using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotNetSqlFactory.DataOperations;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DotNetSqlFactory.Tests
{
    [TestClass]
    public class SqlFactory_Tests
    {
        [TestMethod]
        public void SelectQueryIncludesToDataTable_Test()
        {
            var connectionstring = ConfigurationManager.AppSettings["connectionString"];
            List<int> idList = new List<int>
            {
                1,4,6,8
            };
            // build sql parameters
            SqlHelper<int> sqlHelper = new SqlHelper<int>(idList, SqlDbType.Int, "@Value");
            List<SqlParameter> sqlParameters = sqlHelper.ParameterInListHelper();
            SqlFactory sqlFactory = new SqlFactory(SqlFactory.DataProvider.SqlServer, connectionstring);
            var inventoryDataTable = sqlFactory.SqlSelectQuery(sqlParameters, () =>
            {
                string replaceValue = "<<Value>>";
                string sqlQuery = @"SELECT [CarId]
                                    ,[Make]
                                    ,[Color]
                                    ,[PetName]
                                FROM [BookLearning].[dbo].[Inventory]" + 
                                $"WHERE CarId IN ({replaceValue})";
                return sqlHelper.GenerateSqlCommandTextFromHelper(replaceValue, sqlQuery);
            });
            string allCarIds = "";
            foreach (DataRow row in inventoryDataTable.Rows)
            {
                allCarIds +=  row["CarId"];
            }
            Assert.IsTrue(inventoryDataTable.Rows.Count == 4);
            Assert.IsTrue(allCarIds.Equals(string.Join("", idList.ToArray())));
        }

        [TestMethod]
        public void SelectQueryParamListToDataTable_Test()
        {
            var connectionstring = ConfigurationManager.AppSettings["connectionString"];
            List<SqlParameter> sqlParameters = new List<SqlParameter>()
            { 
                new SqlParameter("@custid", 3),
                new SqlParameter("@carid", 4)
            };

            SqlFactory sqlFactory = new SqlFactory(SqlFactory.DataProvider.SqlServer, connectionstring);
            string sqlQuery = @"SELECT orders.OrderId, 
                                       orders.CustId, 
                                       inv.CarId, 
                                       inv.Color, 
                                       inv.Make, 
                                       inv.PetName
                                FROM BookLearning.dbo.Orders AS orders
                                     LEFT JOIN Inventory AS inv ON inv.CarId = orders.CarId
                                WHERE orders.CustId = @custid
                                      AND inv.CarId = @carid;";
            var customerOrdersDataTable = sqlFactory.SqlSelectQuery(sqlQuery, sqlParameters);
            Assert.IsTrue((int)customerOrdersDataTable.Rows[0]["CarId"] == 4);
            Assert.IsTrue(customerOrdersDataTable.Rows[0]["Make"].ToString().Trim().Equals("Yugo"));
        }

        [TestMethod]
        public void SelectQueryListToDTO_Test()
        {
            var connectionstring = ConfigurationManager.AppSettings["connectionString"];
            List<int> idList = new List<int>
            {
                1,4,6,8
            };
            // build sql parameters
            SqlHelper<int> sqlHelper = new SqlHelper<int>(idList, SqlDbType.Int, "@Value");
            List<SqlParameter> sqlParameters = sqlHelper.ParameterInListHelper();
            SqlFactory sqlFactory = new SqlFactory(SqlFactory.DataProvider.SqlServer, connectionstring);
            var inventoryDataTable = sqlFactory.SqlSelectQuery(sqlParameters, () =>
            {
                string replaceValue = "<<Value>>";
                string sqlQuery = @"SELECT [CarId]
                                    ,[Make]
                                    ,[Color]
                                    ,[PetName]
                                FROM [BookLearning].[dbo].[Inventory]" +
                                $"WHERE CarId IN ({replaceValue})";
                return sqlHelper.GenerateSqlCommandTextFromHelper(replaceValue, sqlQuery);
            });

            // Convert DataTable to class
            DTOMapper<Inventory> dtoMapper = new DTOMapper<Inventory>();
            var inventory = dtoMapper.DataTableToDtoList(inventoryDataTable);
            Assert.IsTrue(inventory.ElementAt(0).CarId.ToString().Equals("1"));
            Assert.IsTrue(inventory.ElementAt(1).Color.ToString().Trim().Equals("Yellow"));
        }

        [TestMethod]
        public void SqlClientDbFactory_Test()
        {
            SqlFactory sqlFactory = new SqlFactory(SqlFactory.DataProvider.SqlServer);
            Assert.IsTrue(sqlFactory.DatabaseFactory.GetType().Name.Equals("SqlClientFactory"));
        }
        [TestMethod]
        public void OracleDbFactory_Test()
        {
            SqlFactory sqlFactory = new SqlFactory(SqlFactory.DataProvider.Oracle);
            Assert.IsTrue(sqlFactory.DatabaseFactory.GetType().Name.Equals("OracleClientFactory"));
        }

        [TestMethod]
        public void OledbDbFactory_Test()
        {
            SqlFactory sqlFactory = new SqlFactory(SqlFactory.DataProvider.OleDb);
            Assert.IsTrue(sqlFactory.DatabaseFactory.GetType().Name.Equals("OleDbFactory"));
        }

        [TestMethod]
        public void OdbcDbFactory_Test()
        {
            SqlFactory sqlFactory = new SqlFactory(SqlFactory.DataProvider.Odbc);
            Assert.IsTrue(sqlFactory.DatabaseFactory.GetType().Name.Equals("OdbcFactory"));
        }
    }

    public class Inventory
    {
        public int CarId { get; set; }
        public string Make { get; set; }
        public string Color { get; set; }
        public string PetName { get; set; }
    }

}
