# Granger
An OWIN based REST and JSON support library.

## Route decorators
* [ ] Collection query parameters
  * [ ] finds collections in json responses
  * [ ] `?start=`, `?limit=`
* [ ] HTTP Method overrides
  * [ ] applies only to POST
  * [ ] `?_method=PUT`
  * [ ] `?_method=DELETE`
* [ ] Conformity Checker
  * [ ] checks all urls are on an "href" key
  * [ ] expand to check responses?
    * [ ] e.g. all 201s have a location?
* [ ] Href fuzzer
  * [ ] Rewrites all hrefs to new generated guids
    * [ ] unsure on when this process gets re-triggered
  * [ ] forces api clients to follow hrefs rather than hard coding
