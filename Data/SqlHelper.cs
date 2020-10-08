//===============================================================================
// Microsoft Data Access Application Block for .NET
// http://msdn.microsoft.com/library/en-us/dnbda/html/daab-rm.asp
//
// SQLHelper.cs
//
// This file contains the implementations of the SqlHelper and SqlHelperParameterCache
// classes.
//
// For more information see the Data Access Application Block Implementation Overview. 
// 
//===============================================================================
// Copyright (C) 2000-2001 Microsoft Corporation
// All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR
// FITNESS FOR A PARTICULAR PURPOSE.
//==============================================================================

using System;
using System.Data;
using System.Collections;
using System.Data.SqlClient;
using Oracle.DataAccess.Client;

namespace QLBH.Core.Data
{
    /// <summary>
    /// The SqlHelper class is intended to encapsulate high performance, scalable best practices for 
    /// common uses of SqlClient.
    /// </summary>
    public sealed class SqlHelper
    {
        #region private utility methods & constructors

        //Since this class provides only static methods, make the default constructor private to prevent 
        //instances from being created with "new SqlHelper()".
        private SqlHelper() { }



        /// <summary>
        /// This method is used to attach array of SqlParameters to a GtidCommand.
        /// 
        /// This method will assign a value of DbNull to any parameter with a direction of
        /// InputOutput and a value of null.  
        /// 
        /// This behavior will prevent default values from being used, but
        /// this will be the less common case than an intended pure output parameter (derived as InputOutput)
        /// where the user provided no input value.
        /// </summary>
        /// <param name="command">The command to which the parameters will be added</param>
        /// <param name="commandParameters">an array of SqlParameters tho be added to command</param>
        private static void AttachParameters(GtidCommand command, GtidParameter[] commandParameters)
        {
            //GtidCommandBuilder.DeriveParameters(command.InnerCommand);
            //if (commandParameters != null && commandParameters.Length > 0)
            //{
            //    for (int i = 0; i < commandParameters.Length; i++)
            //    {
            //        if (command.Parameters.Count > i)
            //            command.Parameters[i].Value = commandParameters[i].Value;
            //    }
            //}

            foreach (GtidParameter p in commandParameters)
            {
                //check for derived output value with no value assigned
                if ((p.Direction == ParameterDirection.InputOutput) && (p.Value == null))
                {
                    p.Value = DBNull.Value;
                }

                command.Parameters.Add(p);
            }
        }

        /// <summary>
        /// This method assigns an array of values to an array of SqlParameters.
        /// </summary>
        /// <param name="commandParameters">array of SqlParameters to be assigned values</param>
        /// <param name="parameterValues">array of objects holding the values to be assigned</param>
        private static void AssignParameterValues(GtidParameter[] commandParameters, object[] parameterValues)
        {
            if ((commandParameters == null) || (parameterValues == null))
            {
                //do nothing if we get no data
                return;
            }

            // we must have the same number of values as we pave parameters to put them in
            if (commandParameters.Length != parameterValues.Length)
            {
                throw new ArgumentException("Parameter count does not match Parameter Value count.");
            }

            //iterate through the SqlParameters, assigning the values from the corresponding position in the 
            //value array
            for (int i = 0, j = commandParameters.Length; i < j; i++)
            {
                commandParameters[i].Value = parameterValues[i];
            }
        }

        /// <summary>
        /// This method opens (if necessary) and assigns a connection, transaction, command type and parameters 
        /// to the provided command.
        /// </summary>
        /// <param name="command">the GtidCommand to be prepared</param>
        /// <param name="connection">a valid GtidConnection, on which to execute this command</param>
        /// <param name="transaction">a valid GtidTransaction, or 'null'</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParameters to be associated with the command or 'null' if no parameters are required</param>
        private static void PrepareCommand(GtidCommand command, GtidConnection connection, GtidTransaction transaction, CommandType commandType, string commandText, GtidParameter[] commandParameters, out bool mustCloseConnection)
        {
            //if the provided connection is not open, we will open it
            if (connection.State != ConnectionState.Open)
            {
                mustCloseConnection = true;
                connection.Open();
            }
            else { mustCloseConnection = false; }

            //associate the connection with the command
            command.Connection = connection;

            //set the command text (stored procedure name or SQL statement)
            command.CommandText = commandText;

            //if we were provided a transaction, assign it.
            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            //set the command type
            command.CommandType = commandType;

            //attach the command parameters if they are provided
            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }

            return;
        }


        #endregion private utility methods & constructors

        #region ExecuteNonQuery

        // Jan 20, 2007 - Sadeer
        // Added the ExecuteDeleteQuery method to support the History (Audit Trail) feature

        /// <summary>
        /// Execute a SQL delete statement against the database specified in the connection string. 
        /// </summary>
        /// <param name="connectionString">The given connection string to connect to the database</param>
        /// <param name="deleteText">The SQL DELETE statement to execute</param>
        /// <param name="userID">The 24/7 ID of the user invoking the DELETE</param>
        /// <returns>An integer representing the number of rows affected by the DELETE command</returns>
        /// hah commented this
        //public static int ExecuteDeleteQuery(string connectionString, string deleteText, int userID)
        //{
        //    // This function assumes that the given SQL query is actually a SQL DELETE statement.
        //    string strDeleteText = deleteText.Trim().ToUpper();
        //    if (0 != strDeleteText.IndexOf("DELETE")) return -1;

        //    // DELETE FROM table WHERE c1 = v1 AND ...
        //    int iStartIndex = strDeleteText.IndexOf("FROM") + "FROM ".Length;
        //    int iEndIndex = strDeleteText.IndexOf("WHERE");

        //    string strTableName = strDeleteText.Substring(iStartIndex, iEndIndex - iStartIndex);

        //    // Prepare the UPDATE statement
        //    string strUpdateText = @"UPDATE " + strTableName +
        //        @" SET [TimeStamp] = GetDate() AND [User_ID] = @UserID";

        //    GtidParameter oParam = new GtidParameter("@UserID", userID);

        //    // Execute the UPDATE statement
        //    ExecuteNonQuery(connectionString, CommandType.Text, strUpdateText, oParam);

        //    // Finally, execute the DELETE statement
        //    return ExecuteNonQuery(connectionString, CommandType.Text, deleteText, oParam);
        //}

        // End - Jan 20, 2007

        // Jan 29, 2007 - Sadeer

        /// <summary>
        /// Execute a SQL delete statement against the database specified in the transaction. 
        /// </summary>
        /// <param name="transaction">The transaction this DELETE statement will be part of</param>
        /// <param name="deleteText">The SQL DELETE statement to execute</param>
        /// <param name="userID">The 24/7 ID of the user invoking the DELETE</param>
        /// <returns>An integer representing the number of rows affected by the DELETE command</returns>
        public static int ExecuteDeleteQuery(GtidTransaction transaction, string deleteText, int userID)
        {
            // This function assumes that the given SQL query is actually a SQL DELETE statement.
            string strDeleteText = deleteText.Trim().ToUpper();
            if (0 != strDeleteText.IndexOf("DELETE")) return -1;

            // DELETE FROM table WHERE c1 = v1 AND ...
            int iStartIndex = strDeleteText.IndexOf("FROM") + "FROM ".Length;
            int iEndIndex = strDeleteText.IndexOf("WHERE");

            string strTableName = strDeleteText.Substring(iStartIndex, iEndIndex - iStartIndex);

            // Prepare the UPDATE statement
            string strUpdateText = @"UPDATE " + strTableName +
                @" SET [TimeStamp] = GetDate() AND [User_ID] = @UserID";

            GtidParameter oParam = new GtidParameter("@UserID", userID);

            // Finally, execute the DELETE statement
            return ExecuteNonQuery(transaction, CommandType.Text, deleteText, oParam);
        }

        // End - Jan 29, 2007

        /// <summary>
        /// Execute a GtidCommand (that returns no resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a GtidConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        /// hah commented this
        //public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        //{
        //    //pass through the call providing null for the set of SqlParameters
        //    return ExecuteNonQuery(connectionString, commandType, commandText, (GtidParameter[])null);
        //}

        /// <summary>
        /// Execute a GtidCommand (that returns no resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new GtidParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a GtidConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        /// hah commented this
        //public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params GtidParameter[] commandParameters)
        //{
        //    //create & open a GtidConnection, and dispose of it after we are done.
        //    using (GtidConnection cn = new GtidConnection(connectionString))
        //    {
        //        cn.Open();

        //        //call the overload that takes a connection in place of the connection string
        //        return ExecuteNonQuery(cn, commandType, commandText, commandParameters);
        //    }
        //}

        /// <summary>
        /// Execute a stored procedure via a GtidCommand (that returns no resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a GtidConnection</param>
        /// <param name="spName">the name of the stored prcedure</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        /// hah commented this
        //public static int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
        //{
        //    //if we receive parameter values, we need to figure out where they go
        //    if ((parameterValues != null) && (parameterValues.Length > 0))
        //    {
        //        //pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
        //        GtidParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

        //        //assign the provided values to these parameters based on parameter order
        //        AssignParameterValues(commandParameters, parameterValues);

        //        //call the overload that takes an array of SqlParameters
        //        return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, commandParameters);
        //    }
        //    //otherwise we can just call the SP without params
        //    else
        //    {
        //        return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
        //    }
        //}

        /// <summary>
        /// Execute a GtidCommand (that returns no resultset and takes no parameters) against the provided GtidConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="connection">a valid GtidConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(GtidConnection connection, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of SqlParameters
            return ExecuteNonQuery(connection, commandType, commandText, (GtidParameter[])null);
        }

        /// <summary>
        /// Execute a GtidCommand (that returns no resultset) against the specified GtidConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders", new GtidParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">a valid GtidConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(GtidConnection connection, CommandType commandType, string commandText, params GtidParameter[] commandParameters)
        {
            //create a command and prepare it for execution
            //GtidCommand cmd = new GtidCommand();
            GtidCommand cmd = connection.CreateCommand();

            bool mustCloseConnection= false;
            
            PrepareCommand(cmd, connection, (GtidTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            //finally, execute the command.
            int retval = cmd.ExecuteNonQuery();

            // detach the SqlParameters from the command object, so they can be used again.
            cmd.Parameters.Clear();
            if (mustCloseConnection)
                connection.Close();
            return retval;
        }

        /// <summary>
        /// Execute a stored procedure via a GtidCommand (that returns no resultset) against the specified GtidConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">a valid GtidConnection</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        /// hah commented this
        //public static int ExecuteNonQuery(GtidConnection connection, string spName, params object[] parameterValues)
        //{
        //    //if we receive parameter values, we need to figure out where they go
        //    if ((parameterValues != null) && (parameterValues.Length > 0))
        //    {
        //        //pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
        //        GtidParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

        //        //assign the provided values to these parameters based on parameter order
        //        AssignParameterValues(commandParameters, parameterValues);

        //        //call the overload that takes an array of SqlParameters
        //        return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
        //    }
        //    //otherwise we can just call the SP without params
        //    else
        //    {
        //        return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
        //    }
        //}

        /// <summary>
        /// Execute a GtidCommand (that returns no resultset and takes no parameters) against the provided GtidTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="transaction">a valid GtidTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(GtidTransaction transaction, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of SqlParameters
            return ExecuteNonQuery(transaction, commandType, commandText, (GtidParameter[])null);
        }

        /// <summary>
        /// Execute a GtidCommand (that returns no resultset) against the specified GtidTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "GetOrders", new GtidParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">a valid GtidTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(GtidTransaction transaction, CommandType commandType, string commandText, params GtidParameter[] commandParameters)
        {
            //create a command and prepare it for execution
            //GtidCommand cmd = new GtidCommand();
            GtidCommand cmd = transaction.Connection.CreateCommand();
            bool mustCloseConnection= false;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            //finally, execute the command.
            int retval = cmd.ExecuteNonQuery();

            // detach the SqlParameters from the command object, so they can be used again.
            cmd.Parameters.Clear();
            return retval;
        }

        /// <summary>
        /// Execute a stored procedure via a GtidCommand (that returns no resultset) against the specified 
        /// GtidTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, trans, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">a valid GtidTransaction</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        /// hah commented this
        //public static int ExecuteNonQuery(GtidTransaction transaction, string spName, params object[] parameterValues)
        //{
        //    //if we receive parameter values, we need to figure out where they go
        //    if ((parameterValues != null) && (parameterValues.Length > 0))
        //    {
        //        //pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
        //        GtidParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

        //        //assign the provided values to these parameters based on parameter order
        //        AssignParameterValues(commandParameters, parameterValues);

        //        //call the overload that takes an array of SqlParameters
        //        return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters);
        //    }
        //    //otherwise we can just call the SP without params
        //    else
        //    {
        //        return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
        //    }
        //}


        #endregion ExecuteNonQuery

        #region ExecuteDataSet

        /// <summary>
        /// Execute a GtidCommand (that returns a resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a GtidConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <returns>a dataset containing the resultset generated by the command</returns>
        /// hah commented this
        //public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
        //{
        //    //pass through the call providing null for the set of SqlParameters
        //    return ExecuteDataset(connectionString, commandType, commandText, (GtidParameter[])null);
        //}

        /// <summary>
        /// Execute a GtidCommand (that returns a resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders", new GtidParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a GtidConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>a dataset containing the resultset generated by the command</returns>
        /// hah commented this
        //public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params GtidParameter[] commandParameters)
        //{
        //    //create & open a GtidConnection, and dispose of it after we are done.
        //    using (GtidConnection cn = new GtidConnection(connectionString))
        //    {
        //        cn.Open();

        //        //call the overload that takes a connection in place of the connection string
        //        return ExecuteDataset(cn, commandType, commandText, commandParameters);
        //    }
        //}

        /// <summary>
        /// Execute a stored procedure via a GtidCommand (that returns a resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(connString, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a GtidConnection</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>a dataset containing the resultset generated by the command</returns>
        /// hah commented this
        //public static DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
        //{
        //    //if we receive parameter values, we need to figure out where they go
        //    if ((parameterValues != null) && (parameterValues.Length > 0))
        //    {
        //        //pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
        //        GtidParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

        //        //assign the provided values to these parameters based on parameter order
        //        AssignParameterValues(commandParameters, parameterValues);

        //        //call the overload that takes an array of SqlParameters
        //        return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, commandParameters);
        //    }
        //    //otherwise we can just call the SP without params
        //    else
        //    {
        //        return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
        //    }
        //}

        /// <summary>
        /// Execute a GtidCommand (that returns a resultset and takes no parameters) against the provided GtidConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">a valid GtidConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <returns>a dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(GtidConnection connection, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of SqlParameters
            return ExecuteDataset(connection, commandType, commandText, (GtidParameter[])null);
        }

        /// <summary>
        /// Execute a GtidCommand (that returns a resultset) against the specified GtidConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new GtidParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">a valid GtidConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>a dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(GtidConnection connection, CommandType commandType, string commandText, params GtidParameter[] commandParameters)
        {
            //create a command and prepare it for execution
            //GtidCommand cmd = new GtidCommand();
            GtidCommand cmd = connection.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, connection, (GtidTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            //create the DataAdapter & DataSet
            GtidDataAdapter da = new GtidDataAdapter(cmd);
            DataSet ds = new DataSet();
            cmd.CommandTimeout = 400;
            //fill the DataSet using default values for DataTable names, etc.
            da.Fill(ds);

            // detach the SqlParameters from the command object, so they can be used again.            
            cmd.Parameters.Clear();
            if (mustCloseConnection)
                connection.Close();
            //return the dataset
            return ds;
        }

        /// <summary>
        /// Execute a stored procedure via a GtidCommand (that returns a resultset) against the specified GtidConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">a valid GtidConnection</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>a dataset containing the resultset generated by the command</returns>
        /// hah commented this
        //public static DataSet ExecuteDataset(GtidConnection connection, string spName, params object[] parameterValues)
        //{
        //    //if we receive parameter values, we need to figure out where they go
        //    if ((parameterValues != null) && (parameterValues.Length > 0))
        //    {
        //        //pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
        //        GtidParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

        //        //assign the provided values to these parameters based on parameter order
        //        AssignParameterValues(commandParameters, parameterValues);

        //        //call the overload that takes an array of SqlParameters
        //        return ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
        //    }
        //    //otherwise we can just call the SP without params
        //    else
        //    {
        //        return ExecuteDataset(connection, CommandType.StoredProcedure, spName);
        //    }
        //}

        /// <summary>
        /// Execute a GtidCommand (that returns a resultset and takes no parameters) against the provided GtidTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">a valid GtidTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <returns>a dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(GtidTransaction transaction, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of SqlParameters
            return ExecuteDataset(transaction, commandType, commandText, (GtidParameter[])null);
        }

        /// <summary>
        /// Execute a GtidCommand (that returns a resultset) against the specified GtidTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new GtidParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">a valid GtidTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>a dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(GtidTransaction transaction, CommandType commandType, string commandText, params GtidParameter[] commandParameters)
        {
            //create a command and prepare it for execution
            //GtidCommand cmd = new GtidCommand();
            GtidCommand cmd = transaction.Connection.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            //create the DataAdapter & DataSet
            GtidDataAdapter da = new GtidDataAdapter(cmd);
            DataSet ds = new DataSet();

            //fill the DataSet using default values for DataTable names, etc.
            da.Fill(ds);

            // detach the SqlParameters from the command object, so they can be used again.
            cmd.Parameters.Clear();

            //return the dataset
            return ds;
        }

        /// <summary>
        /// Execute a stored procedure via a GtidCommand (that returns a resultset) against the specified 
        /// GtidTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">a valid GtidTransaction</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>a dataset containing the resultset generated by the command</returns>
        /// hah commented this
        //public static DataSet ExecuteDataset(GtidTransaction transaction, string spName, params object[] parameterValues)
        //{
        //    //if we receive parameter values, we need to figure out where they go
        //    if ((parameterValues != null) && (parameterValues.Length > 0))
        //    {
        //        //pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
        //        GtidParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

        //        //assign the provided values to these parameters based on parameter order
        //        AssignParameterValues(commandParameters, parameterValues);

        //        //call the overload that takes an array of SqlParameters
        //        return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
        //    }
        //    //otherwise we can just call the SP without params
        //    else
        //    {
        //        return ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
        //    }
        //}

        #endregion ExecuteDataSet

        #region ExecuteReader

        /// <summary>
        /// this enum is used to indicate whether the connection was provided by the caller, or created by SqlHelper, so that
        /// we can set the appropriate CommandBehavior when calling ExecuteReader()
        /// </summary>
        private enum SqlConnectionOwnership
        {
            /// <summary>Connection is owned and managed by SqlHelper</summary>
            Internal,
            /// <summary>Connection is owned and managed by the caller</summary>
            External
        }

        /// <summary>
        /// Create and prepare a GtidCommand, and call ExecuteReader with the appropriate CommandBehavior.
        /// </summary>
        /// <remarks>
        /// If we created and opened the connection, we want the connection to be closed when the DataReader is closed.
        /// 
        /// If the caller provided the connection, we want to leave it to them to manage.
        /// </remarks>
        /// <param name="connection">a valid GtidConnection, on which to execute this command</param>
        /// <param name="transaction">a valid GtidTransaction, or 'null'</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParameters to be associated with the command or 'null' if no parameters are required</param>
        /// <param name="connectionOwnership">indicates whether the connection parameter was provided by the caller, or created by SqlHelper</param>
        /// <returns>IDataReader containing the results of the command</returns>
        private static IDataReader ExecuteReader(GtidConnection connection, GtidTransaction transaction, CommandType commandType, string commandText, GtidParameter[] commandParameters, SqlConnectionOwnership connectionOwnership)
        {
            //create a command and prepare it for execution
            //GtidCommand cmd = new GtidCommand();
            GtidCommand cmd = connection.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            //create a reader
            IDataReader dr;

            // call ExecuteReader with the appropriate CommandBehavior
            if (connectionOwnership == SqlConnectionOwnership.External)
            {
                dr = cmd.ExecuteReader();
            }
            else
            {
                dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }

            // detach the SqlParameters from the command object, so they can be used again.
            cmd.Parameters.Clear();
            if (mustCloseConnection)
                connection.Close();
            return dr;
        }

        /// <summary>
        /// Execute a GtidCommand (that returns a resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  IDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a GtidConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <returns>a IDataReader containing the resultset generated by the command</returns>
        /// hah commented this
        //public static IDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
        //{
        //    //pass through the call providing null for the set of SqlParameters
        //    return ExecuteReader(connectionString, commandType, commandText, (GtidParameter[])null);
        //}

        /// <summary>
        /// Execute a GtidCommand (that returns a resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  IDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders", new GtidParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a GtidConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the Gtidmmand<param>
        /// <returns>a IDataReader containing the resultset generated by the command</returns>
        /// hah commented this
        //public static IDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params GtidParameter[] commandParameters)
        //{
        //    //create & open a GtidConnection
        //    GtidConnection cn = new GtidConnection(connectionString);
        //    cn.Open();

        //    try
        //    {
        //        //call the private overload that takes an internally owned connection in place of the connection string
        //        return ExecuteReader(cn, null, commandType, commandText, commandParameters, SqlConnectionOwnership.Internal);
        //    }
        //    catch
        //    {
        //        //if we fail to return the SqlDatReader, we need to close the connection ourselves
        //        cn.Close();
        //        throw;
        //    }
        //}

        /// <summary>
        /// Execute a stored procedure via a GtidCommand (that returns a resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  IDataReader dr = ExecuteReader(connString, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a GtidConnection</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>a IDataReader containing the resultset generated by the command</returns>
        /// hah commented this
        //public static IDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
        //{
        //    //if we receive parameter values, we need to figure out where they go
        //    if ((parameterValues != null) && (parameterValues.Length > 0))
        //    {
        //        //pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
        //        GtidParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

        //        //assign the provided values to these parameters based on parameter order
        //        AssignParameterValues(commandParameters, parameterValues);

        //        //call the overload that takes an array of SqlParameters
        //        return ExecuteReader(connectionString, CommandType.StoredProcedure, spName, commandParameters);
        //    }
        //    //otherwise we can just call the SP without params
        //    else
        //    {
        //        return ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
        //    }
        //}

        /// <summary>
        /// Execute a GtidCommand (that returns a resultset and takes no parameters) against the provided GtidConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  IDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">a valid GtidConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <returns>a IDataReader containing the resultset generated by the command</returns>
        public static IDataReader ExecuteReader(GtidConnection connection, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of SqlParameters
            return ExecuteReader(connection, commandType, commandText, (GtidParameter[])null);
        }

        /// <summary>
        /// Execute a GtidCommand (that returns a resultset) against the specified GtidConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  IDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders", new GtidParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">a valid GtidConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>a IDataReader containing the resultset generated by the command</returns>
        public static IDataReader ExecuteReader(GtidConnection connection, CommandType commandType, string commandText, params GtidParameter[] commandParameters)
        {
            //pass through the call to the private overload using a null transaction value and an externally owned connection
            return ExecuteReader(connection, (GtidTransaction)null, commandType, commandText, commandParameters, SqlConnectionOwnership.External);
        }

        /// <summary>
        /// Execute a stored procedure via a GtidCommand (that returns a resultset) against the specified GtidConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  IDataReader dr = ExecuteReader(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">a valid GtidConnection</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>a IDataReader containing the resultset generated by the command</returns>
        /// hah commented this
        //public static IDataReader ExecuteReader(GtidConnection connection, string spName, params object[] parameterValues)
        //{
        //    //if we receive parameter values, we need to figure out where they go
        //    if ((parameterValues != null) && (parameterValues.Length > 0))
        //    {
        //        GtidParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

        //        AssignParameterValues(commandParameters, parameterValues);

        //        return ExecuteReader(connection, CommandType.StoredProcedure, spName, commandParameters);
        //    }
        //    //otherwise we can just call the SP without params
        //    else
        //    {
        //        return ExecuteReader(connection, CommandType.StoredProcedure, spName);
        //    }
        //}

        /// <summary>
        /// Execute a GtidCommand (that returns a resultset and takes no parameters) against the provided GtidTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  IDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">a valid GtidTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <returns>a IDataReader containing the resultset generated by the command</returns>
        public static IDataReader ExecuteReader(GtidTransaction transaction, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of SqlParameters
            return ExecuteReader(transaction, commandType, commandText, (GtidParameter[])null);
        }

        /// <summary>
        /// Execute a GtidCommand (that returns a resultset) against the specified GtidTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///   IDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders", new GtidParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">a valid GtidTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>a IDataReader containing the resultset generated by the command</returns>
        public static IDataReader ExecuteReader(GtidTransaction transaction, CommandType commandType, string commandText, params GtidParameter[] commandParameters)
        {
            //pass through to private overload, indicating that the connection is owned by the caller
            return ExecuteReader(transaction.Connection, transaction, commandType, commandText, commandParameters, SqlConnectionOwnership.External);
        }

        ///// <summary>
        ///// Execute a stored procedure via a GtidCommand (that returns a resultset) against the specified
        ///// GtidTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        ///// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///// </summary>
        ///// <remarks>
        ///// This method provides no access to output parameters or the stored procedure's return value parameter.
        ///// 
        ///// e.g.:  
        /////  IDataReader dr = ExecuteReader(trans, "GetOrders", 24, 36);
        ///// </remarks>
        ///// <param name="transaction">a valid GtidTransaction</param>
        ///// <param name="spName">the name of the stored procedure</param>
        ///// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        ///// <returns>a IDataReader containing the resultset generated by the command</returns>
        ///// hah commented this
        //public static IDataReader ExecuteReader(GtidTransaction transaction, string spName, params object[] parameterValues)
        //{
        //    //if we receive parameter values, we need to figure out where they go
        //    if ((parameterValues != null) && (parameterValues.Length > 0))
        //    {
        //        GtidParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

        //        AssignParameterValues(commandParameters, parameterValues);

        //        return ExecuteReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
        //    }
        //    //otherwise we can just call the SP without params
        //    else
        //    {
        //        return ExecuteReader(transaction, CommandType.StoredProcedure, spName);
        //    }
        //}

        #endregion ExecuteReader

        #region ExecuteScalar

        /// <summary>
        /// Execute a GtidCommand (that returns a 1x1 resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a GtidConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
        /// hah commented this
        //public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
        //{
        //    //pass through the call providing null for the set of SqlParameters
        //    return ExecuteScalar(connectionString, commandType, commandText, (GtidParameter[])null);
        //}

        /// <summary>
        /// Execute a GtidCommand (that returns a 1x1 resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount", new GtidParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a GtidConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
        /// hah commented this
        //public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params GtidParameter[] commandParameters)
        //{
        //    //create & open a GtidConnection, and dispose of it after we are done.
        //    using (GtidConnection cn = new GtidConnection(connectionString))
        //    {
        //        cn.Open();

        //        //call the overload that takes a connection in place of the connection string
        //        return ExecuteScalar(cn, commandType, commandText, commandParameters);
        //    }
        //}

        /// <summary>
        /// Execute a stored procedure via a GtidCommand (that returns a 1x1 resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(connString, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a GtidConnection</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
        /// hah commented this
        //public static object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
        //{
        //    //if we receive parameter values, we need to figure out where they go
        //    if ((parameterValues != null) && (parameterValues.Length > 0))
        //    {
        //        //pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
        //        GtidParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

        //        //assign the provided values to these parameters based on parameter order
        //        AssignParameterValues(commandParameters, parameterValues);

        //        //call the overload that takes an array of SqlParameters
        //        return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, commandParameters);
        //    }
        //    //otherwise we can just call the SP without params
        //    else
        //    {
        //        return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
        //    }
        //}

        /// <summary>
        /// Execute a GtidCommand (that returns a 1x1 resultset and takes no parameters) against the provided GtidConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="connection">a valid GtidConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(GtidConnection connection, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of SqlParameters
            return ExecuteScalar(connection, commandType, commandText, (GtidParameter[])null);
        }

        /// <summary>
        /// Execute a GtidCommand (that returns a 1x1 resultset) against the specified GtidConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount", new GtidParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">a valid GtidConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(GtidConnection connection, CommandType commandType, string commandText, params GtidParameter[] commandParameters)
        {
            //create a command and prepare it for execution
            //GtidCommand cmd = new GtidCommand();
            GtidCommand cmd = connection.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, connection, (GtidTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            //execute the command & return the results
            object retval = cmd.ExecuteScalar();

            // detach the SqlParameters from the command object, so they can be used again.
            cmd.Parameters.Clear();
            if (mustCloseConnection)
                connection.Close();
            return retval;

        }

        /// <summary>
        /// Execute a stored procedure via a GtidCommand (that returns a 1x1 resultset) against the specified GtidConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(conn, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="connection">a valid GtidConnection</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
        /// hah commented this
        //public static object ExecuteScalar(GtidConnection connection, string spName, params object[] parameterValues)
        //{
        //    //if we receive parameter values, we need to figure out where they go
        //    if ((parameterValues != null) && (parameterValues.Length > 0))
        //    {
        //        //pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
        //        GtidParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

        //        //assign the provided values to these parameters based on parameter order
        //        AssignParameterValues(commandParameters, parameterValues);

        //        //call the overload that takes an array of SqlParameters
        //        return ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
        //    }
        //    //otherwise we can just call the SP without params
        //    else
        //    {
        //        return ExecuteScalar(connection, CommandType.StoredProcedure, spName);
        //    }
        //}

        /// <summary>
        /// Execute a GtidCommand (that returns a 1x1 resultset and takes no parameters) against the provided GtidTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="transaction">a valid GtidTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(GtidTransaction transaction, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of SqlParameters
            return ExecuteScalar(transaction, commandType, commandText, (GtidParameter[])null);
        }

        /// <summary>
        /// Execute a GtidCommand (that returns a 1x1 resultset) against the specified GtidTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount", new GtidParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">a valid GtidTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(GtidTransaction transaction, CommandType commandType, string commandText, params GtidParameter[] commandParameters)
        {
            //create a command and prepare it for execution
            //GtidCommand cmd = new GtidCommand();
            GtidCommand cmd = transaction.Connection.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            //execute the command & return the results
            object retval = cmd.ExecuteScalar();

            // detach the SqlParameters from the command object, so they can be used again.
            cmd.Parameters.Clear();
            return retval;
        }

        ///// <summary>
        ///// Execute a stored procedure via a GtidCommand (that returns a 1x1 resultset) against the specified
        ///// GtidTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        ///// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///// </summary>
        ///// <remarks>
        ///// This method provides no access to output parameters or the stored procedure's return value parameter.
        ///// 
        ///// e.g.:  
        /////  int orderCount = (int)ExecuteScalar(trans, "GetOrderCount", 24, 36);
        ///// </remarks>
        ///// <param name="transaction">a valid GtidTransaction</param>
        ///// <param name="spName">the name of the stored procedure</param>
        ///// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        ///// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
        ///// hah commented this
        //public static object ExecuteScalar(GtidTransaction transaction, string spName, params object[] parameterValues)
        //{
        //    //if we receive parameter values, we need to figure out where they go
        //    if ((parameterValues != null) && (parameterValues.Length > 0))
        //    {
        //        //pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
        //        GtidParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

        //        //assign the provided values to these parameters based on parameter order
        //        AssignParameterValues(commandParameters, parameterValues);

        //        //call the overload that takes an array of SqlParameters
        //        return ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
        //    }
        //    //otherwise we can just call the SP without params
        //    else
        //    {
        //        return ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
        //    }
        //}

        #endregion ExecuteScalar

        #region ExecuteXmlReader

        /// <summary>
        /// Execute a GtidCommand (that returns a resultset and takes no parameters) against the provided GtidConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  XmlReader r = ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">a valid GtidConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command using "FOR XML AUTO"</param>
        /// <returns>an XmlReader containing the resultset generated by the command</returns>
        //public static XmlReader ExecuteXmlReader(GtidConnection connection, CommandType commandType, string commandText)
        //{
        //    //pass through the call providing null for the set of SqlParameters
        //    return ExecuteXmlReader(connection, commandType, commandText, (GtidParameter[])null);
        //}

        /// <summary>
        /// Execute a GtidCommand (that returns a resultset) against the specified GtidConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  XmlReader r = ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders", new GtidParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">a valid GtidConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command using "FOR XML AUTO"</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an XmlReader containing the resultset generated by the command</returns>
        //public static XmlReader ExecuteXmlReader(GtidConnection connection, CommandType commandType, string commandText, params GtidParameter[] commandParameters)
        //{
        //    //create a command and prepare it for execution
        //    GtidCommand cmd = new GtidCommand();
        //    PrepareCommand(cmd, connection, (GtidTransaction)null, commandType, commandText, commandParameters);

        //    //create the DataAdapter & DataSet
        //    XmlReader retval = cmd.ExecuteXmlReader();

        //    // detach the SqlParameters from the command object, so they can be used again.
        //    cmd.Parameters.Clear();
        //    return retval;

        //}

        /// <summary>
        /// Execute a stored procedure via a GtidCommand (that returns a resultset) against the specified GtidConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  XmlReader r = ExecuteXmlReader(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">a valid GtidConnection</param>
        /// <param name="spName">the name of the stored procedure using "FOR XML AUTO"</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>an XmlReader containing the resultset generated by the command</returns>
        //public static XmlReader ExecuteXmlReader(GtidConnection connection, string spName, params object[] parameterValues)
        //{
        //    //if we receive parameter values, we need to figure out where they go
        //    if ((parameterValues != null) && (parameterValues.Length > 0))
        //    {
        //        //pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
        //        GtidParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

        //        //assign the provided values to these parameters based on parameter order
        //        AssignParameterValues(commandParameters, parameterValues);

        //        //call the overload that takes an array of SqlParameters
        //        return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName, commandParameters);
        //    }
        //    //otherwise we can just call the SP without params
        //    else
        //    {
        //        return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName);
        //    }
        //}

        /// <summary>
        /// Execute a GtidCommand (that returns a resultset and takes no parameters) against the provided GtidTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  XmlReader r = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">a valid GtidTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command using "FOR XML AUTO"</param>
        /// <returns>an XmlReader containing the resultset generated by the command</returns>
        //public static XmlReader ExecuteXmlReader(GtidTransaction transaction, CommandType commandType, string commandText)
        //{
        //    //pass through the call providing null for the set of SqlParameters
        //    return ExecuteXmlReader(transaction, commandType, commandText, (GtidParameter[])null);
        //}

        /// <summary>
        /// Execute a GtidCommand (that returns a resultset) against the specified GtidTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  XmlReader r = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders", new GtidParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">a valid GtidTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command using "FOR XML AUTO"</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an XmlReader containing the resultset generated by the command</returns>
        //public static XmlReader ExecuteXmlReader(GtidTransaction transaction, CommandType commandType, string commandText, params GtidParameter[] commandParameters)
        //{
        //    //create a command and prepare it for execution
        //    GtidCommand cmd = new GtidCommand();
        //    PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);

        //    //create the DataAdapter & DataSet
        //    XmlReader retval = cmd.ExecuteXmlReader();

        //    // detach the SqlParameters from the command object, so they can be used again.
        //    cmd.Parameters.Clear();
        //    return retval;
        //}

        /// <summary>
        /// Execute a stored procedure via a GtidCommand (that returns a resultset) against the specified 
        /// GtidTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  XmlReader r = ExecuteXmlReader(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">a valid GtidTransaction</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>a dataset containing the resultset generated by the command</returns>
        //public static XmlReader ExecuteXmlReader(GtidTransaction transaction, string spName, params object[] parameterValues)
        //{
        //    //if we receive parameter values, we need to figure out where they go
        //    if ((parameterValues != null) && (parameterValues.Length > 0))
        //    {
        //        //pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
        //        GtidParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

        //        //assign the provided values to these parameters based on parameter order
        //        AssignParameterValues(commandParameters, parameterValues);

        //        //call the overload that takes an array of SqlParameters
        //        return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
        //    }
        //    //otherwise we can just call the SP without params
        //    else
        //    {
        //        return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName);
        //    }
        //}


        #endregion ExecuteXmlReader
    }

    /// <summary>
    /// SqlHelperParameterCache provides functions to leverage a static cache of procedure parameters, and the
    /// ability to discover parameters for stored procedures at run-time.
    /// </summary>
    public sealed class SqlHelperParameterCache
    {
        #region private methods, variables, and constructors

        //Since this class provides only static methods, make the default constructor private to prevent 
        //instances from being created with "new SqlHelperParameterCache()".
        private SqlHelperParameterCache() { }

        private static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// resolve at run time the appropriate set of SqlParameters for a stored procedure
        /// </summary>
        /// <param name="connectionString">a valid connection string for a GtidConnection</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">whether or not to include their return value parameter</param>
        /// <returns></returns>
        /// hah commented this
        //private static GtidParameter[] DiscoverSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
        //{
        //    using (GtidConnection cn = new GtidConnection(connectionString))
        //    using (GtidCommand cmd = new GtidCommand(spName, cn))
        //    {
        //        cn.Open();
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        GtidCommandBuilder.DeriveParameters(cmd);

        //        if (!includeReturnValueParameter)
        //        {
        //            cmd.Parameters.RemoveAt(0);
        //        }

        //        GtidParameter[] discoveredParameters = new GtidParameter[cmd.Parameters.Count]; ;

        //        cmd.Parameters.CopyTo(discoveredParameters, 0);

        //        return discoveredParameters;
        //    }
        //}

        //deep copy of cached GtidParameter array
        private static GtidParameter[] CloneParameters(GtidParameter[] originalParameters)
        {
            GtidParameter[] clonedParameters = new GtidParameter[originalParameters.Length];

            for (int i = 0, j = originalParameters.Length; i < j; i++)
            {
                clonedParameters[i] = (GtidParameter)((ICloneable)originalParameters[i]).Clone();
            }

            return clonedParameters;
        }

        #endregion private methods, variables, and constructors

        #region caching functions

        /// <summary>
        /// add parameter array to the cache
        /// </summary>
        /// <param name="connectionString">a valid connection string for a GtidConnection</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters to be cached</param>
        public static void CacheParameterSet(string connectionString, string commandText, params GtidParameter[] commandParameters)
        {
            string hashKey = connectionString + ":" + commandText;

            paramCache[hashKey] = commandParameters;
        }

        /// <summary>
        /// retrieve a parameter array from the cache
        /// </summary>
        /// <param name="connectionString">a valid connection string for a GtidConnection</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <returns>an array of SqlParamters</returns>
        public static GtidParameter[] GetCachedParameterSet(string connectionString, string commandText)
        {
            string hashKey = connectionString + ":" + commandText;

            GtidParameter[] cachedParameters = (GtidParameter[])paramCache[hashKey];

            if (cachedParameters == null)
            {
                return null;
            }
            else
            {
                return CloneParameters(cachedParameters);
            }
        }

        #endregion caching functions

        #region Parameter Discovery Functions

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a GtidConnection</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <returns>an array of SqlParameters</returns>
        /// hah commented this
        //public static GtidParameter[] GetSpParameterSet(string connectionString, string spName)
        //{
        //    return GetSpParameterSet(connectionString, spName, false);
        //}

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a GtidConnection</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">a bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>an array of SqlParameters</returns>
        /// hah commented this
        //public static GtidParameter[] GetSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
        //{
        //    string hashKey = connectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : "");

        //    GtidParameter[] cachedParameters;

        //    cachedParameters = (GtidParameter[])paramCache[hashKey];

        //    if (cachedParameters == null)
        //    {
        //        cachedParameters = (GtidParameter[])(paramCache[hashKey] = DiscoverSpParameterSet(connectionString, spName, includeReturnValueParameter));
        //    }

        //    return CloneParameters(cachedParameters);
        //}

        #endregion Parameter Discovery Functions

    }
}

