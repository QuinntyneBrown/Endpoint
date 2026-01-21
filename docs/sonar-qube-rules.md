# SonarQube Rules Reference

## C# and TypeScript Static Code Analysis Rules

**Source:** [SonarSource Rules](https://rules.sonarsource.com/)  
**Generated:** January 2026

---

## Table of Contents

1. [C# Rules (494 total)](#c-rules)
   - [Vulnerabilities (47)](#c-vulnerabilities)
   - [Bugs (88)](#c-bugs)
   - [Security Hotspots (24)](#c-security-hotspots)
   - [Code Smells (335)](#c-code-smells)
2. [TypeScript Rules (427 total)](#typescript-rules)
   - [Vulnerabilities (30)](#typescript-vulnerabilities)
   - [Bugs (72)](#typescript-bugs)
   - [Security Hotspots (62)](#typescript-security-hotspots)
   - [Code Smells (263)](#typescript-code-smells)
3. [Quick Reference URLs](#quick-reference-urls)

---

# C# Rules

**Total Rules: 494** | Vulnerability: 47 | Bug: 88 | Security Hotspot: 24 | Code Smell: 335 | Quick Fix: 61

## C# Vulnerabilities

| Rule ID | Title |
|---------|-------|
| S7714 | XSLT Transformations should not be vulnerable to injection attacks |
| S7044 | Server-side requests should not be vulnerable to traversing attacks |
| S7039 | Content Security Policies should be restrictive |
| S6781 | JWT secret keys should not be disclosed |
| S6776 | Stack traces should not be disclosed |
| S6680 | Loop boundaries should not be vulnerable to injection attacks |
| S6641 | Connection strings should not be vulnerable to injections attacks |
| S6639 | Memory allocations should not be vulnerable to Denial of Service attacks |
| S6549 | Accessing files should not lead to filesystem oracle attacks |
| S6547 | Environment variables should not be defined from untrusted input |
| S6399 | XML operations should not be vulnerable to injection attacks |
| S6377 | XML signatures should be validated securely |
| S6287 | Applications should not create session cookies from untrusted input |
| S6173 | Reflection should not be vulnerable to injection attacks |
| S6096 | Extracting archives should not lead to zip slip vulnerabilities |
| S5883 | OS commands should not be vulnerable to argument injection attacks |
| S5773 | Types allowed to be deserialized should be restricted |
| S5659 | JWT should be signed and verified with strong cipher algorithms |
| S5547 | Cipher algorithms should be robust |
| S5542 | Encryption algorithms should be used with secure mode and padding scheme |
| S5445 | Insecure temporary file creation methods should not be used |
| S5344 | Passwords should not be stored in plaintext or with a fast hashing algorithm |
| S5334 | Dynamic code execution should not be vulnerable to injection attacks |
| S5147 | NoSQL operations should not be vulnerable to injection attacks |
| S5146 | HTTP request redirections should not be open to forging attacks |
| S5145 | Logging should not be vulnerable to injection attacks |
| S5144 | Server-side requests should not be vulnerable to forging attacks |
| S5135 | Deserialization should not be vulnerable to injection attacks |
| S5131 | Endpoints should not be vulnerable to reflected cross-site scripting (XSS) attacks |
| S4830 | Server certificates should be verified during SSL/TLS connections |
| S4433 | LDAP connections should be authenticated |
| S4426 | Cryptographic keys should be robust |
| S4423 | Weak SSL/TLS protocols should not be used |
| S4347 | Secure random number generators should not output predictable values |
| S4212 | Serialization constructors should be secured |
| S4211 | Members should not have conflicting transparency annotations |
| S3884 | "CoSetProxyBlanket" and "CoInitializeSecurity" should not be used |
| S3649 | Database queries should not be vulnerable to injection attacks |
| S3329 | Cipher Block Chaining IVs should be unpredictable |
| S2755 | XML parsers should not be vulnerable to XXE attacks |
| S2631 | Regular expressions should not be vulnerable to Denial of Service attacks |
| S2115 | A secure password should be used when connecting to a database |
| S2091 | XPath expressions should not be vulnerable to injection attacks |
| S2083 | I/O function calls should not be vulnerable to path injection attacks |
| S2078 | LDAP queries should not be vulnerable to injection attacks |
| S2076 | OS commands should not be vulnerable to command injection attacks |
| S2053 | Password hashing functions should use an unpredictable salt |

## C# Bugs

| Rule ID | Title |
|---------|-------|
| S7133 | Locks should be released within the same method |
| S7131 | A write lock should not be released when a read lock has been acquired and vice versa |
| S6930 | Backslash should be avoided in route templates |
| S6800 | Component parameter type should match the route parameter type constraint |
| S6798 | [JSInvokable] attribute should only be used on public methods |
| S6797 | Blazor query parameter type should be supported |
| S6677 | Message template placeholders should be unique |
| S6674 | Log message template should be syntactically correct |
| S6507 | Blocks should not be synchronized on local variables |
| S5856 | Regular expressions should be syntactically valid |
| S4586 | Non-async "Task/Task<T>" methods should not return null |
| S4583 | Calls to delegate's method "BeginInvoke" should be paired with calls to "EndInvoke" |
| S4428 | "PartCreationPolicyAttribute" should be used with "ExportAttribute" |
| S4277 | "Shared" parts should not be created with "new" |
| S4275 | Getters and setters should access the expected fields |
| S4260 | "ConstructorArgument" parameters should exist in constructors |
| S4210 | Windows Forms entry points should be marked with STAThread |
| S4159 | Classes should implement their "ExportAttribute" interfaces |
| S4158 | Empty collections should not be accessed or iterated |
| S4143 | Collection elements should not be replaced unconditionally |
| S3984 | Exceptions should not be created without being thrown |
| S3981 | Collection sizes and array length comparisons should make sense |
| S3949 | Calculations should not overflow |
| S3927 | Serialization event handlers should be implemented correctly |
| S3926 | Deserialization methods should be provided for "OptionalField" members |
| S3923 | All branches in a conditional structure should not have exactly the same implementation |
| S3903 | Types should be defined in named namespaces |
| S3889 | "Thread.Resume" and "Thread.Suspend" should not be used |
| S3887 | Mutable, non-private fields should not be "readonly" |
| S3869 | "SafeHandle.DangerousGetHandle" should not be called |
| S3655 | Empty nullable value should not be accessed |
| S3610 | Nullable type comparison should not be redundant |
| S3603 | Methods with "Pure" attribute should return a value |
| S3598 | One-way "OperationContract" methods should have "void" return type |
| S3466 | Optional parameters should be passed to "base" calls |
| S3464 | Type inheritance should not be recursive |
| S3456 | "string.ToCharArray()" and "ReadOnlySpan<T>.ToArray()" should not be called redundantly |
| S3453 | Classes should not have only "private" constructors |
| S3449 | Right operands of shift operators should be integers |
| S3397 | "base.Equals" should not be used to check for reference equality in "Equals" if "base" is not "object" |
| S3363 | Date and time should not be used as a type for primary keys |
| S3346 | Expressions used in "Debug.Assert" should not produce side effects |
| S3343 | Caller information parameters should come at the end of the parameter list |
| S3263 | Static fields should appear in the order they must be initialized |
| S3249 | Classes directly extending "object" should not call "base" in "GetHashCode" or "Equals" |
| S3244 | Anonymous delegates should not be used to unsubscribe from Events |
| S3172 | Delegates should not be subtracted |
| S3168 | "async" methods should not return "void" |
| S3005 | "ThreadStatic" should not be used on non-static fields |
| S2997 | "IDisposables" created in a "using" statement should not be returned |
| S2996 | "ThreadStatic" fields should not be initialized |
| S2995 | "Object.ReferenceEquals" should not be used for value types |
| S2955 | Generic parameters not constrained to reference types should not be compared to "null" |
| S2952 | Classes should "Dispose" of members from the classes' own "Dispose" methods |
| S2934 | Property assignments should not be made for "readonly" fields not constrained to reference types |
| S2931 | Classes with "IDisposable" members should implement "IDisposable" |
| S2930 | "IDisposables" should be disposed |
| S2857 | SQL keywords should be delimited by whitespace |
| S2761 | Doubled prefix operators "!!" and "~~" should not be used |
| S2757 | Non-existent operators like "=+" should not be used |
| S2688 | "NaN" should not be used in comparisons |
| S2674 | The length returned from a stream read should be checked |
| S2583 | Conditionally executed code should be reachable |
| S2551 | Shared resources should not be used for locking |
| S2445 | Blocks should be synchronized on read-only fields |
| S2345 | Flags enumerations should explicitly initialize all their members |
| S2328 | "GetHashCode" should not reference mutable fields |
| S2275 | Composite format strings should not lead to unexpected behavior at runtime |
| S2259 | Null pointers should not be dereferenced |
| S2252 | For-loop conditions should be true at least once |
| S2251 | A "for" loop update clause should move the counter in the right direction |
| S2225 | "ToString()" method should not return null |
| S2222 | Locks should be released on all paths |
| S2201 | Methods without side effects should not have their return values ignored |
| S2190 | Loops and recursions should not be infinite |
| S2184 | Results of integer division should not be assigned to floating point variables |
| S2183 | Integral numbers should not be shifted by zero or more than their number of bits-1 |
| S2123 | Values should not be uselessly incremented |
| S2114 | Collections should not be passed as arguments to their own methods |
| S1862 | Related "if/else if" statements should not have the same condition |
| S1848 | Objects should not be created to be dropped immediately without being used |
| S1764 | Identical expressions should not be used on both sides of operators |
| S1751 | Loops with at most one iteration should be refactored |
| S1656 | Variables should not be self-assigned |
| S1244 | Floating point numbers should not be tested for equality |
| S1226 | Method parameters, caught exceptions and foreach variables' initial values should not be ignored |
| S1206 | "Equals(Object)" and "GetHashCode()" should be overridden in pairs |
| S1048 | Finalizers should not throw exceptions |

## C# Security Hotspots

| Rule ID | Title |
|---------|-------|
| S6640 | Using unsafe code blocks is security-sensitive |
| S6444 | Not specifying a timeout for regular expressions is security-sensitive |
| S6418 | Hard-coded secrets are security-sensitive |
| S6350 | Constructing arguments of system commands from user input is security-sensitive |
| S5766 | Creating Serializable objects without data validation checks is security-sensitive |
| S5753 | Disabling ASP.NET "Request Validation" feature is security-sensitive |
| S5693 | Allowing requests with excessive content length is security-sensitive |
| S5443 | Using publicly writable directories is security-sensitive |
| S5332 | Using clear-text protocols is security-sensitive |
| S5122 | Having a permissive Cross-Origin Resource Sharing policy is security-sensitive |
| S5042 | Expanding archive files without controlling resource consumption is security-sensitive |
| S4792 | Configuring loggers is security-sensitive |
| S4790 | Using weak hashing algorithms is security-sensitive |
| S4507 | Delivering code in production with debug features activated is security-sensitive |
| S4502 | Disabling CSRF protections is security-sensitive |
| S4036 | Searching OS commands in PATH is security-sensitive |
| S3330 | Creating cookies without the "HttpOnly" flag is security-sensitive |
| S2612 | Setting loose file permissions is security-sensitive |
| S2257 | Using non-standard cryptographic algorithms is security-sensitive |
| S2245 | Using pseudorandom number generators (PRNGs) is security-sensitive |
| S2092 | Creating cookies without the "secure" flag is security-sensitive |
| S2077 | Formatting SQL queries is security-sensitive |
| S2068 | Hard-coded credentials are security-sensitive |
| S1313 | Using hardcoded IP addresses is security-sensitive |

## C# Code Smells

| Rule ID | Title |
|---------|-------|
| S927 | Parameter names should match base declaration and other partial definitions |
| S907 | "goto" statement should not be used |
| S881 | Increment (++) and decrement (--) operators should not be used in a method call or mixed with other operators in an expression |
| S818 | Literal suffixes should be upper case |
| S7130 | First/Single should be used instead of FirstOrDefault/SingleOrDefault on collections that are known to be non-empty |
| S6968 | Actions that return a value should be annotated with ProducesResponseTypeAttribute containing the return type |
| S6967 | ModelState.IsValid should be called in controller actions |
| S6966 | Awaitable method should be used |
| S6965 | REST API actions should be annotated with an HTTP verb attribute |
| S6964 | Value type property used as input in a controller action should be nullable, required or annotated with the JsonRequiredAttribute to avoid under-posting |
| S6962 | You should pool HTTP connections with HttpClientFactory |
| S6961 | API Controllers should derive from ControllerBase instead of Controller |
| S6960 | Controllers should not have mixed responsibilities |
| S6934 | A Route attribute should be added to the controller when a route template is specified at the action level |
| S6932 | Use model binding instead of reading raw request data |
| S6931 | ASP.NET controller actions should not have a route template starting with "/" |
| S6803 | Parameters with SupplyParameterFromQuery attribute should be used only in routable components |
| S6802 | Using lambda expressions in loops should be avoided in Blazor markup section |
| S6678 | Use PascalCase for named placeholders |
| S6675 | "Trace.WriteLineIf" should not be used with "TraceSwitch" levels |
| S6673 | Log message template placeholders should be in the right order |
| S6672 | Generic logger injection should match enclosing type |
| S6670 | "Trace.Write" and "Trace.WriteLine" should not be used |
| S6669 | Logger field or property name should comply with a naming convention |
| S6668 | Logging arguments should be passed to the correct parameter |
| S6667 | Logging in a catch clause should pass the caught exception as a parameter |
| S6664 | The code block contains too many logging calls |
| S6618 | "string.Create" should be used instead of "FormattableString" |
| S6617 | "Contains" should be used instead of "Any" for simple equality checks |
| S6613 | "First" and "Last" properties of "LinkedList" should be used instead of the "First()" and "Last()" extension methods |
| S6612 | The lambda parameter should be used instead of capturing arguments in "ConcurrentDictionary" methods |
| S6610 | "StartsWith" and "EndsWith" overloads that take a "char" should be used instead of the ones that take a "string" |
| S6609 | "Min/Max" properties of "Set" types should be used instead of the "Enumerable" extension methods |
| S6608 | Prefer indexing instead of "Enumerable" methods on types implementing "IList" |
| S6607 | The collection should be filtered before sorting by using "Where" before "OrderBy" |
| S6605 | Collection-specific "Exists" method should be used instead of the "Any" extension |
| S6603 | The collection-specific "TrueForAll" method should be used instead of the "All" extension |
| S6602 | "Find" method should be used instead of the "FirstOrDefault" extension |
| S6588 | Use the "UnixEpoch" field instead of creating "DateTime" instances that point to the beginning of the Unix epoch |
| S6585 | Don't hardcode the format when turning dates and times to strings |
| S6580 | Use a format provider when parsing date and time |
| S6575 | Use "TimeZoneInfo.FindSystemTimeZoneById" without converting the timezones with "TimezoneConverter" |
| S6566 | Use "DateTimeOffset" instead of "DateTime" |
| S6563 | Use UTC when recording DateTime instants |
| S6562 | Always set the "DateTimeKind" when creating new "DateTime" instances |
| S6561 | Avoid using "DateTime.Now" for benchmarking or timing operations |
| S6513 | "ExcludeFromCodeCoverage" attributes should include a justification |
| S6424 | Interfaces for durable entities should satisfy the restrictions |
| S6423 | Azure Functions should log all failures |
| S6422 | Calls to "async" methods should not be blocking in Azure Functions |
| S6421 | Azure Functions should use Structured Error Handling |
| S6420 | Client instances should not be recreated on each Azure Function invocation |
| S6419 | Azure Functions should be stateless |
| S6354 | Use a testable date/time provider |
| S5034 | "ValueTask" should be consumed correctly |
| S4663 | Comments should not be empty |
| S4635 | Start index should be used instead of calling Substring |
| S4581 | "new Guid()" should not be used |
| S4545 | "DebuggerDisplayAttribute" strings should reference existing members |
| S4524 | "default" clauses should be first or last |
| S4487 | Unread "private" fields should be removed |
| S4462 | Calls to "async" methods should not be blocking |
| S4457 | Parameter validation in "async"/"await" methods should be wrapped |
| S4456 | Parameter validation in yielding methods should be wrapped |
| S4261 | Methods should be named according to their synchronicities |
| S4226 | Extensions should be in separate namespaces |
| S4225 | Extension methods should not extend "object" |
| S4220 | Events should have proper arguments |
| S4214 | "P/Invoke" methods should not be visible |
| S4201 | Null checks should not be combined with "is" operator checks |
| S4200 | Native methods should be wrapped |
| S4144 | Methods should not have identical implementations |
| S4136 | Method overloads should be grouped together |
| S4070 | Non-flags enums should not be marked with "FlagsAttribute" |
| S4069 | Operator overloads should have named alternatives |
| S4061 | "params" should be used instead of "varargs" |
| S4060 | Non-abstract attributes should be sealed |
| S4059 | Property names should not match get methods |
| S4058 | Overloads with a "StringComparison" parameter should be used |
| S4057 | Locales should be set for data types |
| S4056 | Overloads with a "CultureInfo" or an "IFormatProvider" parameter should be used |
| S4055 | Literals should not be passed as localized parameters |
| S4052 | Types should not extend outdated base types |
| S4050 | Operators should be overloaded consistently |
| S4049 | Properties should be preferred |
| S4047 | Generics should be used when appropriate |
| S4041 | Type names should not match namespaces |
| S4040 | Strings should be normalized to uppercase |
| S4039 | Interface methods should be callable by derived types |
| S4035 | Classes implementing "IEquatable<T>" should be sealed |
| S4027 | Exceptions should provide standard constructors |
| S4026 | Assemblies should be marked with "NeutralResourcesLanguageAttribute" |
| S4025 | Child class fields should not differ from parent class fields only by capitalization |
| S4023 | Interfaces should not be empty |
| S4022 | Enumerations should have "Int32" storage |
| S4019 | Base class methods should not be hidden |
| S4018 | All type parameters should be used in the parameter list to enable type inference |
| S4017 | Method signatures should not contain nested generic types |
| S4016 | Enumeration members should not be named "Reserved" |
| S4015 | Inherited member visibility should not be decreased |
| S4005 | "System.Uri" arguments should be used instead of strings |
| S4004 | Collection properties should be readonly |
| S4002 | Disposable types should declare finalizers |
| S4000 | Pointers to unmanaged memory should not be visible |
| S3998 | Threads should not lock on objects with weak identity |
| S3997 | String URI overloads should call "System.Uri" overloads |
| S3996 | URI properties should not be strings |
| S3995 | URI return values should not be strings |
| S3994 | URI Parameters should not be strings |
| S3993 | Custom attributes should be marked with "System.AttributeUsageAttribute" |
| S3992 | Assemblies should explicitly specify COM visibility |
| S3990 | Assemblies should be marked as CLS compliant |
| S3973 | A conditionally executed single line should be denoted by indentation |
| S3972 | Conditionals should start on new lines |
| S3971 | "GC.SuppressFinalize" should not be called |
| S3967 | Multidimensional arrays should not be used |
| S3966 | Objects should not be disposed more than once |
| S3963 | "static" fields should be initialized inline |
| S3962 | "static readonly" constants should be "const" instead |
| S3956 | "Generic.List" instances should not be part of public APIs |
| S3937 | Number patterns should be regular |
| S3928 | Parameter names used into ArgumentException constructors should match an existing one |
| S3925 | "ISerializable" should be implemented correctly |
| S3909 | Collections should implement the generic interface |
| S3908 | Generic event handlers should be used |
| S3906 | Event Handlers should have the correct signature |
| S3904 | Assemblies should have version information |
| S3902 | "Assembly.GetExecutingAssembly" should not be called |
| S3900 | Arguments of public methods should be validated against null |
| S3898 | Value types should implement "IEquatable<T>" |
| S3897 | Classes that provide "Equals(<T>)" should implement "IEquatable<T>" |
| S3885 | "Assembly.Load" should be used |
| S3881 | "IDisposable" should be implemented correctly |
| S3880 | Finalizers should not be empty |
| S3878 | Arrays should not be created for params parameters |
| S3877 | Exceptions should not be thrown from unexpected methods |
| S3876 | Strings or integral types should be used for indexers |
| S3875 | "operator==" should not be overloaded on reference types |
| S3874 | "out" and "ref" parameters should not be used |
| S3872 | Parameter names should not duplicate the names of their methods |
| S3871 | Exception types should be "public" |
| S3776 | Cognitive Complexity of methods should not be too high |
| S3717 | Track use of "NotImplementedException" |
| S3626 | Jump statements should not be redundant |
| S3604 | Member initializer values should not be redundant |
| S3600 | "params" should not be introduced on overrides |
| S3597 | "ServiceContract" and "OperationContract" attributes should be used together |
| S3532 | Empty "default" clauses should be removed |
| S3459 | Unassigned members should be removed |
| S3458 | Empty "case" clauses that fall through to the "default" should be omitted |
| S3457 | Composite format strings should be used correctly |
| S3451 | "[DefaultValue]" should not be used when "[DefaultParameterValue]" is meant |
| S3450 | Parameters with "[DefaultParameterValue]" attributes should also be marked "[Optional]" |
| S3447 | "[Optional]" should not be used on "ref" or "out" parameters |
| S3445 | Exceptions should not be explicitly rethrown |
| S3444 | Interfaces should not simply inherit from base interfaces with colliding members |
| S3443 | Type should not be examined on "System.Type" instances |
| S3442 | "abstract" classes should not have "public" constructors |
| S3441 | Redundant property names should be omitted in anonymous classes |
| S3440 | Variables should not be checked against the values they're about to be assigned |
| S3433 | Test method signatures should be correct |
| S3431 | "[ExpectedException]" should not be used |
| S3427 | Method overloads with default parameter values should not overlap |
| S3416 | Loggers should be named for their enclosing types |
| S3415 | Assertion arguments should be passed in the correct order |
| S3400 | Methods should not return constants |
| S3398 | "private" methods called only by inner classes should be moved to those classes |
| S3376 | Attribute, EventArgs, and Exception type names should end with the type being extended |
| S3366 | "this" should not be exposed from constructors |
| S3358 | Ternary operators should not be nested |
| S3353 | Unchanged variables should be marked as "const" |
| S3267 | Loops should be simplified with "LINQ" expressions |
| S3265 | Non-flags enums should not be used in bitwise operations |
| S3264 | Events should be invoked |
| S3262 | "params" should be used on overrides |
| S3261 | Namespaces should not be empty |
| S3260 | Non-derived "private" classes and records should be "sealed" |
| S3257 | Declarations and initializations should be as concise as possible |
| S3256 | "string.IsNullOrEmpty" should be used |
| S3254 | Default parameter values should not be passed as arguments |
| S3253 | Constructor and destructor declarations should not be redundant |
| S3251 | Implementations should be provided for "partial" methods |
| S3247 | Duplicate casts should not be made |
| S3246 | Generic type parameters should be co/contravariant when possible |
| S3242 | Method parameters should be declared with base types |
| S3241 | Methods should not return values that are never used |
| S3240 | The simplest possible condition syntax should be used |
| S3237 | "value" contextual keyword should be used |
| S3236 | Caller information arguments should not be provided explicitly |
| S3235 | Redundant parentheses should not be used |
| S3234 | "GC.SuppressFinalize" should not be invoked for types without destructors |
| S3220 | Method calls should not resolve ambiguously to overloads with "params" |
| S3218 | Inner class members should not shadow outer class "static" or type members |
| S3217 | "Explicit" conversions of "foreach" loops should not be used |
| S3216 | "ConfigureAwait(false)" should be used |
| S3215 | "interface" instances should not be cast to concrete types |
| S3169 | Multiple "OrderBy" calls should not be used |
| S3063 | "StringBuilder" data should be used |
| S3060 | "is" should not be used with "this" |
| S3059 | Types should not have members with visibility set higher than the type's visibility |
| S3052 | Members should not be initialized to default values |
| S3011 | Reflection should not be used to increase accessibility of classes, methods, or fields |
| S3010 | Static fields should not be updated in constructors |
| S2971 | LINQ expressions should be simplified |
| S2970 | Assertions should be complete |
| S2953 | Methods named "Dispose" should implement "IDisposable.Dispose" |
| S2933 | Fields that are only assigned in the constructor should be "readonly" |
| S2925 | "Thread.Sleep" should not be used in tests |
| S2760 | Sequential tests should not check the same condition |
| S2743 | Static fields should not be used in generic types |
| S2737 | "catch" clauses should do more than rethrow |
| S2701 | Literal boolean values should not be used in assertions |
| S2699 | Tests should include assertions |
| S2696 | Instance members should not write to "static" fields |
| S2692 | "IndexOf" checks should not be for positive numbers |
| S2681 | Multiline blocks should be enclosed in curly braces |
| S2629 | Logging templates should be constant |
| S2589 | Boolean expressions should not be gratuitous |
| S2486 | Generic exceptions should not be ignored |
| S2479 | Whitespace and control characters in string literals should be explicit |
| S2437 | Unnecessary bit operations should not be performed |
| S2436 | Types and methods should not have too many generic parameters |
| S2387 | Child class fields should not shadow parent class fields |
| S2386 | Mutable fields should not be "public static" |
| S2376 | Write-only properties should not be used |
| S2372 | Exceptions should not be thrown from property getters |
| S2368 | Public methods should not have multidimensional array parameters |
| S2365 | Properties should not make collection or array copies |
| S2360 | Optional parameters should not be used |
| S2357 | Fields should be private |
| S2346 | Flags enumerations zero-value members should be named "None" |
| S2344 | Enumeration type names should not have "Flags" or "Enum" suffixes |
| S2342 | Enumeration types should comply with a naming convention |
| S2339 | Public constant members should not be used |
| S2333 | Redundant modifiers should not be used |
| S2330 | Array covariance should not be used |
| S2327 | "try" statements with identical "catch" and/or "finally" blocks should be merged |
| S2326 | Unused type parameters should be removed |
| S2325 | Methods and properties that don't access instance data should be static |
| S2306 | "async" and "await" should not be used as identifiers |
| S2302 | "nameof" should be used |
| S2292 | Trivial properties should be auto-implemented |
| S2291 | Overflow checking should not be disabled for "Enumerable.Sum" |
| S2290 | Field-like events should not be virtual |
| S2219 | Runtime type checking should be simplified |
| S2198 | Unnecessary mathematical comparisons should not be made |
| S2197 | Modulus results should not be checked for direct equality |
| S2187 | Test classes should contain at least one test case |
| S2178 | Short-circuit logic should be used in boolean contexts |
| S2166 | Classes named like "Exception" should extend "Exception" or a subclass |
| S2156 | "sealed" classes should not have "protected" members |
| S2148 | Underscores should be used to make large numbers readable |
| S2139 | Exceptions should be either logged or rethrown but not both |
| S2094 | Classes should not be empty |
| S1994 | "for" loop increment clauses should modify the loops' counters |
| S1944 | Invalid casts should be avoided |
| S1940 | Boolean checks should not be inverted |
| S1939 | Inheritance list should not be redundant |
| S1905 | Redundant casts should not be used |
| S1871 | Two branches in a conditional structure should not have exactly the same implementation |
| S1858 | "ToString()" calls should not be redundant |
| S1854 | Unused assignments should be removed |
| S1821 | "switch" statements should not be nested |
| S1699 | Constructors should only call non-overridable methods |
| S1698 | "==" should not be used when "Equals" is overridden |
| S1696 | NullReferenceException should not be caught |
| S1694 | An abstract class should have both abstract and concrete methods |
| S1659 | Multiple variables should not be declared on the same line |
| S1643 | Strings should not be concatenated using '+' in a loop |
| S1607 | Tests should not be ignored |
| S1541 | Methods and properties should not be too complex |
| S1481 | Unused local variables should be removed |
| S1479 | "switch" statements with many "case" clauses should have only one statement |
| S1451 | Track lack of copyright and license headers |
| S1450 | Private fields only used as local variables in methods should become local variables |
| S1449 | Culture should be specified for "string" operations |
| S138 | Functions should not have too many lines of code |
| S134 | Control flow statements "if", "switch", "for", "foreach", "while", "do" and "try" should not be nested too deeply |
| S1312 | Logger fields should be "private static readonly" |
| S131 | "switch/Select" statements should contain a "default/Case Else" clauses |
| S1309 | Track uses of in-source issue suppressions |
| S1301 | "switch" statements should have at least 3 "case" clauses |
| S127 | "for" loop stop conditions should be invariant |
| S1264 | A "while" loop should be used instead of a "for" loop |
| S126 | "if ... else if" constructs should end with "else" clauses |
| S125 | Sections of code should not be commented out |
| S1227 | break statements should not be used except for switch cases |
| S122 | Statements should be on separate lines |
| S1215 | "GC.Collect" should not be called |
| S1210 | "Equals" and the comparison operators should be overridden when implementing "IComparable" |
| S121 | Control structures should use curly braces |
| S1200 | Classes should not be coupled to too many other classes |
| S1199 | Nested code blocks should not be used |
| S1192 | String literals should not be duplicated |
| S1186 | Methods should not be empty |
| S1185 | Overriding members should do more than simply call the same member in the base class |
| S1172 | Unused method parameters should be removed |
| S1168 | Empty arrays and collections should be returned instead of null |
| S1163 | Exceptions should not be thrown in finally blocks |
| S1155 | "Any()" should be used to test for emptiness |
| S1151 | "switch case" clauses should not have too many lines of code |
| S1147 | Exit methods should not be called |
| S1144 | Unused private types or members should be removed |
| S1135 | Track uses of "TODO" tags |
| S1134 | Track uses of "FIXME" tags |
| S1133 | Deprecated code should be removed |
| S113 | Files should end with a newline |
| S1128 | Unnecessary "using" should be removed |
| S1125 | Boolean literals should not be redundant |
| S1123 | "Obsolete" attributes should include explanations |
| S1121 | Assignments should not be made from within sub-expressions |
| S112 | General or reserved exceptions should never be thrown |
| S1118 | Utility classes should not have public constructors |
| S1117 | Local variables should not shadow class fields or properties |
| S1116 | Empty statements should be removed |
| S1110 | Redundant pairs of parentheses should be removed |
| S1109 | A close curly brace should be located at the beginning of a line |
| S1104 | Fields should not have public accessibility |
| S110 | Inheritance tree of classes should not be too deep |
| S109 | Magic numbers should not be used |
| S108 | Nested blocks of code should not be left empty |
| S1075 | URIs should not be hardcoded |
| S107 | Methods should not have too many parameters |
| S1067 | Expressions should not be too complex |
| S1066 | Mergeable "if" statements should be combined |
| S106 | Standard outputs should not be used directly to log anything |
| S105 | Tabulation characters should not be used |
| S104 | Files should not have too many lines of code |
| S103 | Lines should not be too long |
| S101 | Types should be named in PascalCase |
| S1006 | Method overrides should not change parameter defaults |
| S100 | Methods and properties should be named in PascalCase |

---

# TypeScript Rules

**Total Rules: 427** | Vulnerability: 30 | Bug: 72 | Security Hotspot: 62 | Code Smell: 263 | Quick Fix: 88

## TypeScript Vulnerabilities

| Rule ID | Title |
|---------|-------|
| S7044 | Server-side requests should not be vulnerable to traversing attacks |
| S6321 | Administration services access should be restricted to specific IP addresses |
| S6317 | AWS IAM policies should limit the scope of permissions given |
| S6287 | Applications should not create session cookies from untrusted input |
| S6105 | DOM updates should not lead to open redirect vulnerabilities |
| S6096 | Extracting archives should not lead to zip slip vulnerabilities |
| S5883 | OS commands should not be vulnerable to argument injection attacks |
| S5876 | A new session should be created during user authentication |
| S5696 | DOM updates should not lead to cross-site scripting (XSS) attacks |
| S5659 | JWT should be signed and verified with strong cipher algorithms |
| S5547 | Cipher algorithms should be robust |
| S5542 | Encryption algorithms should be used with secure mode and padding scheme |
| S5527 | Server hostnames should be verified during SSL/TLS connections |
| S5334 | Dynamic code execution should not be vulnerable to injection attacks |
| S5147 | NoSQL operations should not be vulnerable to injection attacks |
| S5146 | HTTP request redirections should not be open to forging attacks |
| S5144 | Server-side requests should not be vulnerable to forging attacks |
| S5131 | Endpoints should not be vulnerable to reflected cross-site scripting (XSS) attacks |
| S4830 | Server certificates should be verified during SSL/TLS connections |
| S4426 | Cryptographic keys should be robust |
| S4423 | Weak SSL/TLS protocols should not be used |
| S3649 | Database queries should not be vulnerable to injection attacks |
| S2819 | Origins should be verified during cross-origin communications |
| S2817 | Web SQL databases should not be used |
| S2755 | XML parsers should not be vulnerable to XXE attacks |
| S2631 | Regular expressions should not be vulnerable to Denial of Service attacks |
| S2598 | File uploads should be restricted |
| S2083 | I/O function calls should not be vulnerable to path injection attacks |
| S2076 | OS commands should not be vulnerable to command injection attacks |
| S1525 | Debugger statements should not be used |

## TypeScript Bugs

| Rule ID | Title |
|---------|-------|
| S905 | Non-empty statements should change control flow or have at least one side-effect |
| S6959 | "Array.reduce()" calls should include an initial value |
| S6958 | Literals should not be used as functions |
| S6761 | "children" and "dangerouslySetInnerHTML" should not be used together |
| S6757 | "this" should not be used in functional components |
| S6756 | "setState" should use a callback when referencing the previous state |
| S6638 | Binary expressions should not always return the same value |
| S6544 | Promises should not be misused |
| S6534 | Numbers should not lose precision |
| S6523 | Optional chaining should not be used if returning "undefined" throws an error |
| S6443 | React state setter function should not be called with its matching state variable |
| S6442 | React's useState hook should not be used directly in the render function or body of a component |
| S6440 | React Hooks should be properly called |
| S6439 | React components should not render non-boolean condition values |
| S6438 | Comments inside JSX expressions should be enclosed in curly braces |
| S6435 | React "render" functions should return a value |
| S6426 | Exclusive tests should not be commited to version control |
| S6351 | Regular expressions with the global flag should be used with caution |
| S6328 | Replacement strings should reference existing regular expression groups |
| S6324 | Regular expressions should not contain control characters |
| S6323 | Alternation in regular expressions should not contain empty alternatives |
| S6080 | Disabling Mocha timeouts should be explicit |
| S5868 | Unicode Grapheme Clusters should be avoided inside regex character classes |
| S5867 | Regular expressions using Unicode character classes or property escapes should enable the unicode flag |
| S5863 | Assertions should not be given twice the same argument |
| S5856 | Regular expressions should be syntactically valid |
| S5850 | Alternatives in regular expressions should be grouped when used with anchors |
| S5842 | Repeated patterns in regular expressions should not match the empty string |
| S5260 | Table cells should reference their headers |
| S5256 | Tables should have headers |
| S4822 | Promise rejections should not be caught by "try" blocks |
| S4335 | Type intersections should use meaningful types |
| S4275 | Getters and setters should access the expected fields |
| S4158 | Empty collections should not be accessed or iterated |
| S4143 | Collection elements should not be replaced unconditionally |
| S4124 | Constructors should not be declared inside interfaces |
| S3984 | Errors should not be created without being thrown |
| S3981 | Collection size and array length comparisons should make sense |
| S3923 | All branches in a conditional structure should not have exactly the same implementation |
| S3854 | "super()" should be invoked appropriately |
| S3812 | Parentheses should be used when negating "in" and "instanceof" operations |
| S3799 | Destructuring patterns should not be empty |
| S3786 | Template literal placeholder syntax should not be used in regular strings |
| S3699 | The return value of void functions should not be used |
| S3616 | Comma and logical OR operators should not be used in switch cases |
| S3531 | Generators should explicitly "yield" a value |
| S3001 | "delete" should be used only with object properties |
| S2999 | "new" should only be used with functions and classes |
| S2871 | "Array.prototype.sort()" and "Array.prototype.toSorted()" should use a compare function |
| S2757 | Non-existent operators '=+', '=-' and '=!' should not be used |
| S2688 | "NaN" should not be used in comparisons |
| S2639 | Empty character classes should not be used |
| S2427 | The base should be provided to "parseInt" |
| S2424 | Built-in objects should not be overridden |
| S2251 | A "for" loop update clause should move the counter in the right direction |
| S2201 | Return values from functions without side effects should not be ignored |
| S2137 | Special identifiers should not be bound or assigned |
| S2123 | Values should not be uselessly incremented |
| S1862 | "if/else if" chains and "switch" cases should not have the same condition |
| S1848 | Objects should not be created to be dropped immediately without being used |
| S1764 | Identical expressions should not be used on both sides of a binary operator |
| S1763 | All code should be reachable |
| S1751 | Loops with at most one iteration should be refactored |
| S1656 | Variables should not be self-assigned |
| S1535 | "for...in" loops should filter properties before acting on them |
| S1534 | Member names should not be duplicated within a class or object literal |
| S1530 | Function declarations should not be made within blocks |
| S1529 | Bitwise operators should not be used in boolean contexts |
| S1226 | Initial values of parameters, caught exceptions, and loop variables should not be ignored |
| S1154 | Results of operations on strings should not be ignored |
| S1143 | Jump statements should not occur in "finally" blocks |
| S1082 | Mouse events should have corresponding keyboard events |

## TypeScript Security Hotspots

| Rule ID | Title |
|---------|-------|
| S6350 | Constructing arguments of system commands from user input is security-sensitive |
| S6333 | Creating public APIs is security-sensitive |
| S6332 | Using unencrypted EFS file systems is security-sensitive |
| S6330 | Using unencrypted SQS queues is security-sensitive |
| S6329 | Allowing public network access to cloud resources is security-sensitive |
| S6327 | Using unencrypted SNS topics is security-sensitive |
| S6319 | Using unencrypted SageMaker notebook instances is security-sensitive |
| S6308 | Using unencrypted Elasticsearch domains is security-sensitive |
| S6304 | Policies granting access to all resources of an account are security-sensitive |
| S6303 | Using unencrypted RDS DB resources is security-sensitive |
| S6302 | Policies granting all privileges are security-sensitive |
| S6299 | Disabling Vue.js built-in escaping is security-sensitive |
| S6281 | Allowing public ACLs or policies on a S3 bucket is security-sensitive |
| S6275 | Using unencrypted EBS volumes is security-sensitive |
| S6270 | Policies authorizing public access to resources are security-sensitive |
| S6268 | Disabling Angular built-in sanitization is security-sensitive |
| S6265 | Granting access to S3 buckets to all or authenticated users is security-sensitive |
| S6252 | Disabling versioning of S3 buckets is security-sensitive |
| S6249 | Authorizing HTTP communications with S3 buckets is security-sensitive |
| S6245 | Disabling server-side encryption of S3 buckets is security-sensitive |
| S5852 | Using slow regular expressions is security-sensitive |
| S5759 | Forwarding client IP address is security-sensitive |
| S5757 | Allowing confidential information to be logged is security-sensitive |
| S5743 | Allowing browsers to perform DNS prefetching is security-sensitive |
| S5742 | Disabling Certificate Transparency monitoring is security-sensitive |
| S5739 | Disabling Strict-Transport-Security policy is security-sensitive |
| S5736 | Disabling strict HTTP no-referrer policy is security-sensitive |
| S5734 | Allowing browsers to sniff MIME types is security-sensitive |
| S5732 | Disabling content security policy frame-ancestors directive is security-sensitive |
| S5730 | Allowing mixed-content is security-sensitive |
| S5728 | Disabling content security policy fetch directives is security-sensitive |
| S5725 | Using remote artifacts without integrity checks is security-sensitive |
| S5693 | Allowing requests with excessive content length is security-sensitive |
| S5691 | Statically serving hidden files is security-sensitive |
| S5689 | Disclosing fingerprints from web application technologies is security-sensitive |
| S5604 | Using intrusive permissions is security-sensitive |
| S5443 | Using publicly writable directories is security-sensitive |
| S5332 | Using clear-text protocols is security-sensitive |
| S5247 | Disabling auto-escaping in template engines is security-sensitive |
| S5148 | Authorizing an opened window to access back to the originating window is security-sensitive |
| S5122 | Having a permissive Cross-Origin Resource Sharing policy is security-sensitive |
| S5042 | Expanding archive files without controlling resource consumption is security-sensitive |
| S4829 | Reading the Standard Input is security-sensitive |
| S4823 | Using command line arguments is security-sensitive |
| S4818 | Using Sockets is security-sensitive |
| S4817 | Executing XPath expressions is security-sensitive |
| S4790 | Using weak hashing algorithms is security-sensitive |
| S4787 | Encrypting data is security-sensitive |
| S4784 | Using regular expressions is security-sensitive |
| S4721 | Using shell interpreter when executing OS commands is security-sensitive |
| S4507 | Delivering code in production with debug features activated is security-sensitive |
| S4502 | Disabling CSRF protections is security-sensitive |
| S4036 | Searching OS commands in PATH is security-sensitive |
| S3330 | Creating cookies without the "HttpOnly" flag is security-sensitive |
| S2612 | Setting loose POSIX file permissions is security-sensitive |
| S2255 | Writing cookies is security-sensitive |
| S2245 | Using pseudorandom number generators (PRNGs) is security-sensitive |
| S2092 | Creating cookies without the "secure" flag is security-sensitive |
| S2077 | Formatting SQL queries is security-sensitive |
| S2068 | Hard-coded credentials are security-sensitive |
| S1523 | Dynamically executing code is security-sensitive |
| S1313 | Using hardcoded IP addresses is security-sensitive |

## TypeScript Code Smells

| Rule ID | Title |
|---------|-------|
| S909 | "continue" should not be used |
| S888 | Equality operators should not be used in "for" loop termination conditions |
| S881 | Increment (++) and decrement (--) operators should not be used in a method call or mixed with other operators in an expression |
| S878 | Comma operator should not be used |
| S7060 | Module should not import itself |
| S7059 | Constructors should not contain asynchronous operations |
| S6957 | Deprecated React APIs should not be used |
| S6861 | Mutable variables should not be exported |
| S6859 | Imports should not use absolute paths |
| S6853 | Label elements should have a text label and an associated control |
| S6852 | Elements with an interactive role should support focus |
| S6851 | Images should have a non-redundant alternate description |
| S6850 | Heading elements should have accessible content |
| S6848 | Non-interactive DOM elements should not have an interactive handler |
| S6847 | Non-interactive elements shouldn't have event handlers |
| S6846 | DOM elements should not use the "accesskey" property |
| S6845 | Non-interactive DOM elements should not have the `tabIndex` property |
| S6844 | Anchor tags should not be used as buttons |
| S6843 | Interactive DOM elements should not have non-interactive ARIA roles |
| S6842 | Non-interactive DOM elements should not have interactive ARIA roles |
| S6841 | "tabIndex" values should be 0 or -1 |
| S6840 | DOM elements should use the "autocomplete" attribute correctly |
| S6836 | "case" and "default" clauses should not contain lexical declarations |
| S6827 | Anchors should contain accessible content |
| S6825 | Focusable elements should not have "aria-hidden" attribute |
| S6824 | No ARIA role or property for unsupported DOM elements |
| S6823 | DOM elements with the `aria-activedescendant` property should be accessible via the tab key |
| S6822 | No redundant ARIA role |
| S6821 | DOM elements with ARIA roles should have a valid non-abstract role |
| S6819 | Prefer tag over ARIA role |
| S6811 | DOM elements with ARIA role should only have supported properties |
| S6807 | DOM elements with ARIA roles should have the required properties |
| S6793 | ARIA properties in DOM elements should have valid values |
| S6791 | React legacy lifecycle methods should not be used |
| S6790 | String references should not be used |
| S6789 | React's "isMounted" should not be used |
| S6788 | React's "findDOMNode" should not be used |
| S6775 | All "defaultProps" should have non-required PropTypes |
| S6772 | Spacing between inline elements should be explicit |
| S6770 | User-defined JSX components should use Pascal case |
| S6767 | Unused React typed props should be removed |
| S6766 | JSX special characters should be escaped |
| S6763 | "shouldComponentUpdate" should not be defined when extending "React.PureComponent" |
| S6759 | React props should be read-only |
| S6754 | The return value of "useState" should be destructured and named symmetrically |
| S6750 | The return value of "ReactDOM.render" should not be used |
| S6749 | Redundant React fragments should be removed |
| S6748 | React "children" should not be passed as prop |
| S6747 | JSX elements should not use unknown properties and attributes |
| S6746 | In React "this.state" should not be mutated directly |
| S6679 | "Number.isNaN()" should be used to check for "NaN" value |
| S6676 | Calls to ".call()" and ".apply()" methods should not be redundant |
| S6671 | Literals should not be used for promise rejection |
| S6666 | Spread syntax should be used instead of "apply()" |
| S6661 | Object spread syntax should be used instead of "Object.assign" |
| S6660 | If statements should not be the only statement in else blocks |
| S6657 | Octal escape sequences should not be used |
| S6654 | \_\_proto\_\_ property should not be used |
| S6653 | Use Object.hasOwn static method instead of hasOwnProperty |
| S6650 | Renaming import, export, and destructuring assignments should not be to the same name |
| S6647 | Unnecessary constructors should be removed |
| S6644 | Ternary operator should not be used instead of simpler alternatives |
| S6643 | Prototypes of builtin objects should not be modified |
| S6637 | Unnecessary calls to ".bind()" should not be used |
| S6635 | Constructors should not return values |
| S6627 | Users should not use internal APIs |
| S6606 | Nullish coalescing should be preferred |
| S6598 | Function types should be preferred |
| S6594 | "RegExp.exec()" should be preferred over "String.match()" |
| S6590 | "as const" assertions should be preferred |
| S6583 | Enum members should not mix value types |
| S6582 | Optional chaining should be preferred |
| S6578 | Enum values should be unique |
| S6572 | Enum member values should be either all initialized or none |
| S6571 | Type constituents of unions and intersections should not be redundant |
| S6569 | Unnecessary type constraints should be removed |
| S6568 | Non-null assertions should not be used misleadingly |
| S6565 | Prefer the return type "this" in fluent interfaces |
| S6564 | Redundant type aliases should not be used |
| S6557 | Ends of strings should be checked with "startsWith()" and "endsWith()" |
| S6551 | Objects and classes converted or coerced to strings should define a "toString()" method |
| S6550 | All enum members should be literals |
| S6535 | Unnecessary character escapes should be removed |
| S6522 | Import variables should not be reassigned |
| S6509 | Extra boolean casts should be removed |
| S6486 | JSX list components keys should match up between renders |
| S6481 | React Context Provider values should have stable identities |
| S6480 | Disallow `.bind()` and arrow functions in JSX props |
| S6479 | JSX list components should not use array indexes as key |
| S6478 | React components should not be nested |
| S6477 | JSX list components should have a key property |
| S6441 | Unused methods of React components should be removed |
| S6397 | Character classes in regular expressions should not contain only one character |
| S6353 | Regular expression quantifiers and character classes should be used concisely |
| S6331 | Regular expressions should not contain empty groups |
| S6326 | Regular expressions should not contain multiple spaces |
| S6325 | Regular expression literals should be used when possible |
| S6092 | Chai assertions should have only one reason to succeed |
| S6079 | Tests should not execute any code after "done()" is called |
| S6035 | Single-character alternations in regular expressions should be replaced with character classes |
| S6019 | Reluctant quantifiers in regular expressions should be followed by an expression that can't match the empty string |
| S5973 | Tests should be stable |
| S5958 | Tests should check which exception is thrown |
| S5869 | Character classes in regular expressions should not contain the same character twice |
| S5860 | Names of regular expressions named groups should be used |
| S5843 | Regular expressions should not be too complicated |
| S5264 | "<object>" tags should provide an alternative content |
| S5257 | HTML "<table>" should not be used for layout purposes |
| S5254 | HTML elements should have a valid language attribute |
| S4798 | Optional boolean parameters should have default value |
| S4782 | Optional property declarations should not use both '?' and 'undefined' syntax |
| S4634 | Shorthand promises should be used |
| S4624 | Template literals should not be nested |
| S4623 | "undefined" should not be passed as the value of optional parameters |
| S4622 | Union types should not have too many elements |
| S4621 | Union and intersection types should not include duplicated constituents |
| S4619 | "in" should not be used on arrays |
| S4524 | "default" clauses should be last |
| S4328 | Dependencies should be explicit |
| S4327 | "this" should not be assigned to variables |
| S4326 | "await" should not be used redundantly |
| S4325 | Redundant casts and non-null assertions should be avoided |
| S4324 | Primitive return types should be used |
| S4323 | Type aliases should be used |
| S4322 | Type predicates should be used |
| S4204 | The "any" type should not be used |
| S4165 | Assignments should not be redundant |
| S4157 | Default type parameters should be omitted |
| S4156 | "module" should not be used |
| S4144 | Functions should not have identical implementations |
| S4140 | Sparse arrays should not be created with extra commas |
| S4139 | "for in" should not be used with iterables |
| S4138 | "for of" should be used with Iterables |
| S4137 | Type assertions should use "as" |
| S4136 | Method overloads should be grouped together |
| S4123 | "await" should only be used with promises |
| S4084 | Media elements should have captions |
| S4043 | Array-mutating methods should not be used misleadingly |
| S4030 | Collection contents should be used |
| S4023 | Interfaces should not be empty |
| S3973 | A conditionally executed single line should be denoted by indentation |
| S3972 | Conditionals should start on new lines |
| S3863 | Imports from the same module should be merged |
| S3801 | Functions should use "return" consistently |
| S3776 | Cognitive Complexity of functions should not be too high |
| S3735 | "void" should not be used |
| S3723 | Trailing commas should be used |
| S3696 | Literals should not be thrown |
| S3626 | Jump statements should not be redundant |
| S3579 | Array indexes should be numeric |
| S3533 | "import" should be used to include external code |
| S3525 | Class methods should be used instead of "prototype" assignments |
| S3524 | Braces and parentheses should be used consistently with arrow functions |
| S3516 | Function returns should not be invariant |
| S3514 | Destructuring syntax should be used for assignments |
| S3513 | "arguments" should not be accessed directly |
| S3512 | Template strings should be used instead of concatenation |
| S3504 | Variables should be declared with "let" or "const" |
| S3499 | Shorthand object properties should be grouped at the beginning or end of an object declaration |
| S3498 | Object literal shorthand syntax should be used |
| S3415 | Assertion arguments should be passed in the correct order |
| S3402 | Strings and non-strings should not be added |
| S3358 | Ternary operators should not be nested |
| S3353 | Unchanged variables should be marked as "const" |
| S3317 | Default export names and file names should match |
| S3257 | Primitive types should be omitted from initialized or defaulted declarations |
| S3003 | Comparison operators should not be used with strings |
| S2990 | The global "this" object should not be used |
| S2970 | Assertions should be complete |
| S2966 | Non-null assertions should not be used |
| S2933 | Fields that are only assigned in the constructor should be "readonly" |
| S2870 | "delete" should not be used on arrays |
| S2737 | "catch" clauses should do more than rethrow |
| S2699 | Tests should include assertions |
| S2692 | "indexOf" checks should not be for positive numbers |
| S2685 | "arguments.caller" and "arguments.callee" should not be used |
| S2681 | Multiline blocks should be enclosed in curly braces |
| S2589 | Boolean expressions should not be gratuitous |
| S2486 | Exceptions should not be ignored |
| S2430 | Constructor names should start with an upper case letter |
| S2392 | Variables should be used in the blocks where they are declared |
| S2376 | Property getters and setters should come in pairs |
| S2310 | Loop counters should not be assigned within the loop body |
| S2301 | Methods should not contain selector parameters |
| S2260 | JavaScript parser failure |
| S2234 | Parameters should be passed in the correct order |
| S2208 | Wildcard imports should not be used |
| S2187 | Test files should contain at least one test case |
| S2138 | "undefined" should not be assigned |
| S2094 | Classes should not be empty |
| S2004 | Functions should not be nested too deeply |
| S1994 | "for" loop increment clauses should modify the loops' counters |
| S1940 | Boolean checks should not be inverted |
| S1874 | Deprecated APIs should not be used |
| S1871 | Two branches in a conditional structure should not have exactly the same implementation |
| S1854 | Unused assignments should be removed |
| S1821 | "switch" statements should not be nested |
| S1788 | Function parameters with default values should be last |
| S1774 | The ternary operator should not be used |
| S1607 | Tests should not be skipped without providing a reason |
| S1541 | Cyclomatic Complexity of functions should not be too high |
| S1539 | "strict" mode should be used with caution |
| S1537 | Trailing commas should not be used |
| S1533 | Wrapper objects should not be used for primitive types |
| S1528 | Array constructors should not be used |
| S1526 | Variables declared with "var" should be declared before they are used |
| S1516 | Multiline string literals should not be used |
| S1515 | Functions should not be defined inside loops |
| S1488 | Local variables should not be declared and then immediately returned or thrown |
| S1479 | "switch" statements should not have too many "case" clauses |
| S1472 | Function call arguments should not start on new lines |
| S1451 | Track lack of copyright and license headers |
| S1444 | Public "static" fields should be read-only |
| S1441 | Quotes for string literals should be used consistently |
| S1440 | "===" and "!==" should be used instead of "==" and "!=" |
| S1439 | Only "while", "do", "for" and "switch" statements should be labelled |
| S1438 | Statements should end with semicolons |
| S139 | Comments should not be located at the end of lines of code |
| S138 | Functions should not have too many lines of code |
| S135 | Loops should not contain more than a single "break" or "continue" statement |
| S134 | Control flow statements "if", "for", "while", "switch" and "try" should not be nested too deeply |
| S1314 | Octal values should not be used |
| S131 | "switch" statements should have "default" clauses |
| S1301 | "if" statements should be preferred over "switch" when simpler |
| S128 | Switch cases should end with an unconditional "break" statement |
| S1264 | A "while" loop should be used instead of a "for" loop |
| S126 | "if ... else if" constructs should end with "else" clauses |
| S125 | Sections of code should not be commented out |
| S124 | Track comments matching a regular expression |
| S122 | Statements should be on separate lines |
| S1219 | "switch" statements should not contain non-case labels |
| S121 | Control structures should use curly braces |
| S1199 | Nested code blocks should not be used |
| S1192 | String literals should not be duplicated |
| S1186 | Functions should not be empty |
| S1172 | Unused function parameters should be removed |
| S117 | Variable, property and parameter names should comply with a naming convention |
| S1135 | Track uses of "TODO" tags |
| S1134 | Track uses of "FIXME" tags |
| S1131 | Lines should not end with trailing whitespaces |
| S113 | Files should end with a newline |
| S1128 | Unnecessary imports should be removed |
| S1125 | Boolean literals should not be used in comparisons |
| S1121 | Assignments should not be made from within sub-expressions |
| S1119 | Labels should not be used |
| S1117 | Variables should not be shadowed |
| S1116 | Extra semicolons should be removed |
| S1110 | Redundant pairs of parentheses should be removed |
| S1105 | An open curly brace should be located at the end of a line |
| S1090 | iFrames must have a title |
| S109 | Magic numbers should not be used |
| S108 | Nested blocks of code should not be left empty |
| S1077 | Image, area, button with image and object elements should have an alternative text |
| S107 | Functions should not have too many parameters |
| S1068 | Unused private class members should be removed |
| S1067 | Expressions should not be too complex |
| S1066 | Mergeable "if" statements should be combined |
| S106 | Standard outputs should not be used directly to log anything |
| S105 | Tabulation characters should not be used |
| S104 | Files should not have too many lines of code |
| S103 | Lines should not be too long |
| S101 | Class names should comply with a naming convention |
| S100 | Function and method names should comply with a naming convention |

---

# Quick Reference URLs

## Official Resources

| Resource | URL |
|----------|-----|
| C# Rules | https://rules.sonarsource.com/csharp/ |
| TypeScript Rules | https://rules.sonarsource.com/typescript/ |
| All Languages | https://rules.sonarsource.com/ |
| SonarQube Documentation | https://docs.sonarsource.com/sonarqube/ |
| Quality Profiles Guide | https://docs.sonarsource.com/sonarqube/latest/instance-administration/quality-profiles/ |

## Rule Categories by URL

### C#
- Vulnerabilities: https://rules.sonarsource.com/csharp/type/Vulnerability/
- Bugs: https://rules.sonarsource.com/csharp/type/Bug/
- Security Hotspots: https://rules.sonarsource.com/csharp/type/Security%20Hotspot/
- Code Smells: https://rules.sonarsource.com/csharp/type/Code%20Smell/

### TypeScript
- Vulnerabilities: https://rules.sonarsource.com/typescript/type/Vulnerability/
- Bugs: https://rules.sonarsource.com/typescript/type/Bug/
- Security Hotspots: https://rules.sonarsource.com/typescript/type/Security%20Hotspot/
- Code Smells: https://rules.sonarsource.com/typescript/type/Code%20Smell/

---

## Notes

1. **Built-in Profiles**: SonarQube includes "Sonar way" profiles for each language with recommended rules already activated
2. **Profile Export**: Quality profiles can be exported as XML via Quality Profiles → [Profile Name] → Back up
3. **Profile Import**: XML profiles can be imported via Quality Profiles → Restore
4. **Rule Details**: Click any rule ID in the SonarSource Rules website for detailed documentation, code examples, and remediation guidance
5. **Quick Fix**: Rules marked with Quick Fix support automatic remediation suggestions in supported IDEs

---

*Document generated from SonarSource Rules database*