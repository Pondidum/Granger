# Granger
An OWIN based REST and JSON support library.

## Route decorators
* [x] Collection query parameters
  * [x] finds collections in json responses
  * [x] `?start=`, `?limit=`
* [x] HTTP Method overrides
  * [x] applies only to POST
  * [x] `?_method=PUT`
  * [x] `?_method=DELETE`
  * [x] whitelist of allowed method overrides
* [ ] Conformity Checker
  * [ ] checks all urls are on an "href" key
  * [ ] expand to check responses?
    * [ ] e.g. all 201s have a location?
* [ ] Href fuzzer
  * [ ] Rewrites all hrefs to new generated guids
    * [ ] unsure on when this process gets re-triggered
  * [ ] forces api clients to follow hrefs rather than hard coding
