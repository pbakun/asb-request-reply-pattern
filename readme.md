
This repository is sample implementation of Asynchronous Request-Reply pattern in Azure Service Bus.

### First create resource group
```
az group create --name RequestReplyPattern --location 'west europe'
```

### Create Azure Service Bus namespace

```
az servicebus namespace create --name requestreplyasb --resource-group RequestReplyPattern --sku Standard
```

### Create request queue

```
az servicebus queue create --name request-queue --namespace-name requestreplyasb --resource-group RequestReplyPattern
```

### Create reply queue
```
az servicebus queue create --name reply-queue --namespace-name requestreplyasb --resource-group RequestReplyPattern --enable-session true
```

### .NET Project

Run both `Producer` and `Consumer` projects. The first one contains API endpoints. You can send simple message with `HTTP POST` to `/send`, eg. `/send?message=Hello world`.
In the response you should get session id, which using `HTTP GET` request you can use to:
1. Check session state => `/getstate?messageid=<session-id>`
2. receive reply => `/getresponse?messageid=<session-id>`