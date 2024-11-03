# Data anonymization

Using the static `Anonymizer` methods you can anonymize sensitive data before sending it to a client, displaying or storing it:

| Data type | Method | Example |
| --------- | ------ | ------- |
| IP address | `Anonymize(Ip/IPv(4/6))Address` | `8.8.xxx.xxx` |
| Email address | `AnonymizeEmailAddress` | `xliax@domain.com` |
| Credit card number | `AnonymizeCreditCardNumber` | `xxxx xxxx xxxx 1234` |
| Phone number | `AnonymizePhoneNumber` | `+49 6341 xxxxxx` |
| Bank account number | `AnonymizeBankAccountNumber` | `xxxxxx1234` |
| IBAN number | `AnonymizeIban` | `DE123456789012xxxx1234` |
