# SFMC4NET

.NET Core library to handle Data Extension operations with Salesforce Marketing Cloud

## Getting Started

This library will help you to retrieve and upsert data to a Date Extension in Salesforce Marketing Cloud. 

###Entity
A data entity is required to store the rows coming from the Data Extension as well as to upsert rows to a DE.
There are some attributes you can use to decorate the entity:

[DataExtension]
Class level decorator

[DEColumn]
Useful to override the column name, if your entity has a column that maps to a column in the DE with a different name, you can use this one.

[KeyColumn]
Flags the field as a Primary Key column in the DE

[IgnoreColumn]
To ignore fields in the entity

Example:

	[DataExtension]
    internal class SFMC4NET_TestDE
    {
        [KeyColumn]
        public string Id { get; set; }
        
		public string FirstName { get; set; }
        public string LastName { get; set; }
        
		[DEColumn(Name = "Birthdate")]
        public DateTime? TestTime { get; set; }

		[IgnoreColumn]
		public string NonImportantField { get; set; }
    }

###Configuring the DataExtensionManager
This is the main object and it has to be configured to commmunicate with Salesforce Marketing Cloud.

You need to know the Salesforce stack on which your instance is running as well as your API credentials, if not provided, it will default to stack 7.

dataExtensionManager = DataExtensionManager.Build
                .SetStack(stack)
                .UsingCredentials(clientId,secret);

###Retrieving rows
To get data from a DE, you need the DE's External Key:

await dataExtensionManager.GetRows<SFMC4NET_TestDE>(dataExtensionId, string.Empty);

###Sending rows
First you need to build a entity list and then send this list to the DE.

	var list = new List<SFMC4NET_TestDE>();
    var user = new SFMC4NET_TestDE { Id = "2", FirstName = "SecondUser", LastName = "Lastname" };
    list.Add(user);

	await dataExtensionManager.SendRows(dataExtensionId, list);

## Authors

* **David Maldonado** - *Initial work* - [Iqdamn](https://github.com/iqdamn)


## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Hat tip to anyone whose code was used
* Inspiration
* etc
