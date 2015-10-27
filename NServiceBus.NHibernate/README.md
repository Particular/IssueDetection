## NServiceBus.NHibernate Issue 117 Detector

An [issue in multiple versions of NServiceBus.NHibernate] was identified which causes events published to multiple subscribers to be handled successfully by only one subscriber if the Outbox feature is enabled.

The issue was identified in the following versions of NServiceBus.NHibernate:

* 6.2.0-6.2.3, fixed in [6.2.4](https://github.com/Particular/NServiceBus.NHibernate/releases/tag/6.2.4)
* 6.1.0-6.1.3, fixed in [6.1.4](https://github.com/Particular/NServiceBus.NHibernate/releases/tag/6.1.4)
* 6.0.0-6.0.3, fixed in [6.0.4](https://github.com/Particular/NServiceBus.NHibernate/releases/tag/6.0.4)

This tool will examine your ServiceControl instance to see if you're affected.

## How to run the tool

### 1. Shut down Service Control
Open the Services Panel, find the `Particular ServiceControl` instance and stop it. 

### 2. Start Service Control in Maintenance Mode
From an administrative command prompt, run `ServiceControl.exe --maint`. This will expose the embedded RavenDB database via RavenDB Studio (by default at `http://localhost:33333/storage`). ServiceControl will keep processing messages as usual.

### 3. Run the tool from the command line
On the machine that ServiceControl is running, open a command prompt and run it in administrative mode. Navigate to directory where `Issue558Detector.exe` is stored and run it. 
The tool assumes that RavenDB instance is exposed at the default url, i.e. `http://localhost:33333/storage`. However, if you customized the ServiceControl configuration to alter the URL, you can alter the Issue117Detector.exe.config file to point to the customized URL:

	<?xml version="1.0" encoding="utf-8" ?>
	<configuration>
	  <connectionStrings>
		<add name="ServiceControl" connectionString="Url=http://localhost:33333/storage" />
	  </connectionStrings>
	</configuration>

The tool outputs directly to the console window. To keep the output for examination later, pipe the output to a file using the `>` operator like this:

    Issue117Detector.exe > results.txt

After the tool finishes running, press Enter in ServiceControl to exit maintenance mode.

### 4. Restart Service Control
In the Services Control Pane, find the `Particular ServiceControl` instance and restart it.

## About the tool

### Sample output:

```
Creating Temp Indexes (this may take some time)...
Index creation complete.

Messages.TheEvent handled by 2 endpoints:
  Endpoints:
    * MsmqOutboxSubB
    * MsmqOutboxSubA
  Problem Messages:
    * Msg 07106655-0373-4381-bc64-a53e00fc909b not processed by 1 endpoints:
      MsmqOutboxSubA
    * Msg ac200595-32a9-478d-9730-a53e00fc9115 not processed by 1 endpoints:
      MsmqOutboxSubA
    * Msg 0feffd5f-ef83-4ece-a4f4-a53e00fc9139 not processed by 1 endpoints:
      MsmqOutboxSubB
    * Msg b97be7f2-00d6-4219-a2a7-a53e00fc8833 not processed by 1 endpoints:
      MsmqOutboxSubA
    * Msg 6a0e13b9-f2a7-43a1-a3ef-a53e00fc90ef not processed by 1 endpoints:
      MsmqOutboxSubA
    * Msg cfcb6607-7b26-4323-9758-a53e00fc8c43 not processed by 1 endpoints:
      MsmqOutboxSubB
    * Msg 1d5435fd-26e2-463a-9682-a53e00fc915b not processed by 1 endpoints:
      MsmqOutboxSubA
    * Msg eea23c4f-c2f0-4bb2-8841-a53e00fc8de7 not processed by 1 endpoints:
      MsmqOutboxSubB
    * Msg 38da4f0c-3546-4d78-9b13-a53e00fc90cb not processed by 1 endpoints:
      MsmqOutboxSubA

Removing Temp Indexes...
Indexes removed successfully.
Complete. Press Enter to exit.
```
### What does it do?

The tool creates two temporary indexes in the RavenDB database that is embedded within ServiceControl. This index is only required for the tool and will be removed when the tool is done executing. Depending upon the number of messages that you have, this step may take some time. In our testing we have seen it take 10-15 minutes. 

Once the indexes have been created, the tool will use one index to find messages that are events potentially handled by more than one endpoint. Next, it will use the second index to analyze all messages of these types to see which endpoint(s) "missed" processing the event.

If there is no message-specific output, then you are likely not affected. However, it is still possible that, by random chance, every time any event was processed, it was always processed by the same endpoint, and the other endpoint(s) were always behind it in line. In this case, ServiceControl would not know about the additional endpoints that should have processed the message, and therefore, can not report on it.