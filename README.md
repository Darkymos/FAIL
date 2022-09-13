# FAIL
### short for Fast Amazing Intelligent Language

Interpreted language written in [C#](https://dotnet.microsoft.com/). Currently not finished, development continues though.
A compiler may follow in the future.

## Goal

It's main aim is an OOP-language with a static type system and runtime safety as all errors should be catched at compile-time. 
For a later compiler, a memory system comparable to [Rust](https://www.rust-lang.org/) will be implemented to avoid using manual allocations or a `Garbage Collector`.
It could end up as a `Rust` with `OOP`, like `C++` compared to `C`, but with `C#`-like syntaxes, a lot of syntactic sugar and high-level features complemented by a bunch of own ideas.

Another goal is to provide a language framework, on top of which many different languages with unique syntaxes and features can be build, as long as they all output the same `AST` (the same code architecture). With that in mind, even analyzers, type checkers and much more can be used for different lanugages. The compiler and compiled code would be the same to achieve a maximum of interopability.

## Contribution

Feel free to use and report issues!
Contribution is also welcomed, but not everything will be merged, as this project is scoped to learn something about language design and compilers (for now).
Besides that, every contribution will be accepted (as long as it matches the quality standards), just whole new feature-implementations won't be...

## Monetization

Although this project is managed be an organization which is aimed to produce commercial products, this project is completely non-profit, open-source and free to use. Therefore, it is marked with the `MIT`-license. `FAIL` and all of it's related components might be shifted towards their own organization in the future.

## Documentation

Documentation can be found in this repo's wiki, but a dedicated website hosted for free (to ensure staying non-profit) might follow in the future. The documentation is mostly incomplete for now and will be advanced continuously. Though, it is not meant to be complete for now, as `FAIL` is still under heavy development.

## IDE Support

IDE support for `Visual Studio` is in development. At least the `Textmate Grammars` can be reused for other IDEs e.g. `Visual Studio Code`, but analyzers and auto-completion can't be. It has to be mentioned, that IDE support is at this point kind of a 'proof of concept' and not really advanced. To implement IDE support, every contribution is welcomed!
