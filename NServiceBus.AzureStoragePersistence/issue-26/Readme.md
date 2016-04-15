An issue was identified (here) that concurrently starting the same saga instance causes duplicate records to be stored in Azure Storage Persistence.
This tool addresses solving the duplicate problem. Detailed information about processing with Azure storage upgrade and running the deduplication tool can be found in the documentation here:

http://docs.particular.net/nservicebus/upgrades/asp-saga-deduplication
