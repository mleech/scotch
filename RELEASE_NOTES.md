#### 0.2.0 - 2018-04-23
* Update to Netstandard1.6

**NB**: The serialization format of the cassette file has changed slightly. Where headers used to be serialized like this:
```json
    "responseHeaders": [
        {
            "Connection": "keep-alive"
        },
        {
            "Vary": "Origin"
        }
    ],
    "contentHeaders": [
        {
            "Content-Length": "64"
        },
        {
            "Content-Type": "application/json; charset=utf-8"
        },
    ]
```

They are now serialized like this:

```json
    "responseHeaders": {
        "Connection": "keep-alive",
        "Vary": "Origin"
    },
    "contentHeaders": {
        "Content-Length": "64",
        "Content-Type": "application/json; charset=utf-8"
    },
```
Existing cassette files will need to be re-recorded, or edited to match the new format.

#### 0.1.1 - 2017-09-26
* Fix for #1 - Fix issue with ContentType header when replaying request

#### 0.1.0 - 2016-05-06
* Initial version