# log4net.Appenders.Azure

Transfer all your logs to the [Azure Table or Blob Storage](http://azure.microsoft.com/de-de/services/storage/) via Appender for [log4Net](https://logging.apache.org/log4net/), and customise the format of logs

## Install
Add To project via NuGet:  
1. Right click on a project and click 'Manage NuGet Packages'.  
2. Search for 'log4net.Appenders.Azure' and click 'Install'.  

## Configuration
### Table Storage 
Every log entry is stored in a separate row.

	<appender name="AzureTableAppender" type="log4net.Appender.AzureTableAppender, log4net.Appender.Azure">
	   <param name="TableName" value="testLoggingTable"/>
	   <!-- You can either specify a connection string or use the ConnectionStringName property instead -->
	   <param name="ConnectionString" value="UseDevelopmentStorage=true"/>
	   <!--<param name="ConnectionStringName" value="GlobalConfigurationString" />-->
	   <!-- You can specify this to make each LogProperty as separate Column in TableStorage, 
		Default: all Custom Properties were logged into one single field -->
	   <param name="PropAsColumn" value="true" />
	   <param name="PartitionKeyType" value="LoggerName" />
	 </appender>
	
* <b>TableName:</b>  
  Name of the table in Table Storage
* <b>ConnectionString:</b>  
  The full Azure Storage connection string
* <b>ConnectionStringName:</b>  
  Name of a connection string specified under connectionString
* <b>PropAsColumn:</b>  
  <i>Optional</i></br>
  Default: all properties were written in a single field(default).  
  If you specifiy this with the value true then each custom log4net property is logged as separate column/field in the table.  
  Remember that Table storage has a Limit of 255 Properties ([see here](https://azure.microsoft.com/en-us/documentation/articles/storage-table-design-guide/#about-the-azure-table-service)).
* <b>PartitionKeyType:</b>  
  <i>Optional</i></br>
  Default "LoggerName": (each logger gets his own partition in Table Storage)  
  "DateReverse": order by Date Reverse to see the latest items first ([How to order elements by date reverse](http://gauravmantri.com/2012/02/17/effective-way-of-fetching-diagnostics-data-from-windows-azure-diagnostics-table-hint-use-partitionkey/))

	
### BlobStorage
Every log Entry can be stored as a seperate XML, JSON or TXT file.

    <appender name="AzureBlobAppender" type="log4net.Appender.AzureBlobAppender, log4net.Appender.Azure">
      <param name="ContainerName" value="testloggingblob"/>
      <param name="DirectoryName" value="logs"/>
      <param name="FileFormat" value="xml"/>
      <param name="DocumentName" value="logs"/>
      <!-- You can either specify a connection string or use the ConnectionStringName property instead -->
      <param name="ConnectionString" value="UseDevelopmentStorage=true"/>
      <!--<param name="ConnectionStringName" value="GlobalConfigurationString" />-->
    </appender>
	
* <b>ContainerName:</b>  
  Name of the container in Blob Storage	
* <b>DirectoryName:</b>  
  Name of the folder in the specified container
* <b>FileFormat:</b>  
  <i>Optional</i></br>
  Default: XML</br>
  The format of the generated log file, can be either <i>xml</i>, <i>txt</i> or <i>json</i>
* <b>DocumentName:</b>  
  <i>Optional</i></br>
  Default: 'entry.log'</br>
  The name of the generated log file. String format is {DateTime}.{DocumentName}.{FileFormat}
* <b>ConnectionString:</b>  
  The full Azure Storage connection string
* <b>ConnectionStringName:</b>  
  Name of a connection string specified under connectionString

### AppendBlobStorage
Every log Entry can be stored as a seperate XML, JSON or TXT file.

    <appender name="AzureAppendBlobAppender" type="log4net.Appender.AzureAppendBlobAppender, log4net.Appender.Azure">
      <param name="ContainerName" value="testloggingblob"/>
      <param name="DirectoryName" value="logs"/>
      <param name="FileFormat" value="json"/>
      <param name="DocumentName" value="logs"/>
      <!-- You can either specify a connection string or use the ConnectionStringName property instead -->
      <param name="ConnectionString" value="UseDevelopmentStorage=true"/>
      <!--<param name="ConnectionStringName" value="GlobalConfigurationString" />-->
    </appender>
	
* <b>ContainerName:</b>  
  Name of the container in Blob Storage	
* <b>DirectoryName:</b>  
  Name of the folder in the specified container
* <b>FileFormat:</b>  
  <i>Optional</i></br>
  Default: XML</br>
  The format of the generated log file, can be either <i>xml</i>, <i>txt</i> or <i>json</i>
* <b>DocumentName:</b>  
  <i>Optional</i></br>
  Default: 'entry.log'</br>
  The name of the generated log file. String format is {DateTime}.{DocumentName}.{FileFormat}
* <b>ConnectionString:</b>  
  The full Azure Storage connection string
* <b>ConnectionStringName:</b>  
  Name of a connection string specified under connectionString
  
## Formats

Currently XML, JSON and TXT output formats are supported

## Author

**Matthew Mirzai** (https://github.com/TheMirzai)

## Credits

**Karell Ste-Marie** (https://github.com/stemarie) Creator of the original package, which is no longer active.
