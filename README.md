# Nlog.Targets.Fluentd.Core 

A target that writes logs into Fluentd

## Installation
This target is distributed via NuGet package. Install it by running
```
Install-Package Nlog.Targets.Fluentd.Core
``` 

## Usage 
```
using NLog;
using NLog.Fluentd.Target;
using NLog.Fluentd.Target.Sinks.Fluentd;

var fluentdTarget = new FluentdTarget(new FluentdSinkOptions("127.0.0.1",2488,"tag"));
 
```
 
Acknowledgements
-------
This started as a fork of Borys Yermakov's [serilog-sinks-fluentd](https://github.com/borisermakof/serilog-sinks-fluentd) Serilog extension.

 
