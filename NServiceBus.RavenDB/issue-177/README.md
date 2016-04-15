## NServiceBus.RavenDB Issues 177, 209 and 206 Detector

An issue with NServiceBus.RavenDB was identified which causes timeouts and/or sagas to be stored with multiple naming conventions in the RavenDB database. The naming convention used is dependent on whether the end user provides a simple connection string or a custom DocumentStore to NServiceBus. Both are fine, but one can not switch between them without risking losing data. 

The issue is present in all versions of NServiceBus.RavenDB, but from [3.0.7](https://github.com/Particular/NServiceBus.RavenDB/releases/tag/6.2.4) NServiceBus will refuse to start an endpoint with multiple naming conventions in use for the same timeout/saga entity.

This tool will examine a RavenDB instance and detect any databases with multiple naming conventions for any timeout and/or saga documents. 

## How to run the tool

### 1. Pick the right version of the tool
RavenDB Server 2.5, or earlier, must be checked with `CollectionChecker25.exe`.
RavenDB Server 3.0, or later, must be checked with `CollectionChecker30.exe`.

### 2. Run the tool from the command line
Open a command prompt on any machine that has access to the RavenDB instance. Navigate to the directory where `CollectionChecker25.exe` `CollectionChecker30.exe` is stored and run it, providing a connection string to your RavenDB instance as the only argument.
 
The tool outputs directly to the console window. To keep the output for examination later, pipe the output to a file using the `>` operator like this:

    CollectionChecker30.exe http://localhost:8080 > results.txt

### 3. Check the output
The tool will print a report for all RavenDB databases on the server instance. 

## About the tool

### Sample output with no problems detected:

```
********************Checking database DatabaseWithoutProblems for problems.********************************
No problems found in database DatabaseWithoutProblems.
***************Finished checking database DatabaseWithoutProblems for problems.****************************

```

### Sample output with problems detected:

```
********************Checking database DatabaseWithProblems for problems.********************************
Problem! Duplicate timeout data collections found: TimeoutData/TimeoutDatas
Problem! Duplicate saga data collections found: OrderSagaDatas/OrderSaga.
Problem! Duplicate saga data collections found: ShippingSagaDatas/ShippingSaga.
Problems found in database DatabaseWithProblems. There are duplicated timeout and/or saga collections in this database. 
This is caused by switching between using a connection string and providing a full document store to NSB endpoint using 
this database. You need to inspect the collections listed above and decided if you can discard the ones currently not in use.
***************Finished checking database DatabaseWithProblems for problems.****************************
```

### What does it do?

The tool only fetches metadata about all the current collections in the RavenDB databases. It then uses both the NSB and RavenDB naming conventions combined with some smarts to detect if any of them are duplicates. 

It only issues simple metadata read operations and can be run against a live Raven instance with negligible performance implications.
