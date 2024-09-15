# Output Cache

https://learn.microsoft.com/en-us/aspnet/core/performance/caching/output?view=aspnetcore-8.0

### Default output cache rules:
- Only HTTP 200 responses are cached.
- Only HTTP GET or HEAD requests are cached.
- Responses that set cookies aren't cached.
- Responses to authenticated requests aren't cached.

## Start:
- run ``` docker-compose -f redis.yaml up -d```
- use ```http file```

## Redis Commands
- ```keys * ``` lists all keys
- ```del key``` delete specific key
- ```type key``` check type of a key
- ```get key``` get value for a given key (value is a string)