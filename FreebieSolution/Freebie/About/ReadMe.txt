Component: FM.Data 
Createdy By: Francis Marasigan
Description: This is a wrapper on top of DbContext of EntityFramework.

The wrapper was created to provide the following functionality:

1. Fix schema issue for oracle provider (dbo default schema).
2. Support for executing more flexible/relax raw query.
3. Support DataTable and DataSet result that should only for adhoc.
4. Extension methods for getting table name and field mapping to avoid hard coding column/table name on the raw query.

Pre-Requisite of using this component.

1. Latest ODP.Net Provider.
2. EF 4.3.1 (latest)

Note: This wrapper is only compatible 

Sample: 
Scaffold PocoFromDB "User Id={user_id};Password={db_password};Data Source={data_source}" {context_name} {table_name} {entity_name} {db_provider}

{user_id}		Database username
{db_password}	Database password 
{context_name}	Name of your context, ex: FooContext
{table_name}	Name of database table your want to generate. ex: ORDER_DETALS 
{entity_name}	Name of you entity class ex: OrderDetails
{db_provider}	Name of the ADO.Net Provider that you want to use: ex: Oracle.DataAccess.Client  