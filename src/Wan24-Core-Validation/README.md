# wan24-Core-Validation

This library contains validation attributes for 
[`wan24-Core`](https://github.com/WAN-Solutions/wan24-Core) types and 
references the 
[`ObjectValidation` NuGet package](https://github.com/nd1012/ObjectValidation).

## Usage

### How to get it

This package is available as a 
[NuGet package](https://github.com/nd1012/wan24-Core-Validation).

### Validation attributes

| Attribute | Description |
| --------- | ----------- |
| `EndPointAttribute` | Validates an IP (`IPEndPoint`) or host endpoint (`HostEndPoint`) |
| `HostEndPointAttribute` | Validates a host endpoint (`HostEndPoint`) |
| `IpSubNetAttribute` | Validates an `IpSubNet` |
| `RgbAttribute` | Validates a 24 bit unsigned integer RGB value |
| `RuntimeCountLimitAttribute` | Validates a count limit with runtime configured limits (from static properties) |
| `UidAttribute` | Validates an `Uid` |
| `UidExtAttribute` | Validates an `UidExt` |
