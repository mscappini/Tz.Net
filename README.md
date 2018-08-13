# Tz.Net library

Tz.Net is a .NET Standard 2.0 library created for the purpose of interacting with the Tezos blockchain by communicating with the JSON RPC API.

## Features

- Wallet funds transfer
- Wallet activation
- Get wallet balance
- Public/private key generation
- Message signing
- Message verification
- Describe the RPC API schema
- Get head chain/block
- Get network stats
- ... and other less notable functions

## Disclaimer

This library is currently in **beta**!

Many features are still being developed, are not yet fully implemented, and may not work 100% of the time. *To date, this has only been tested on the alphanet and zeronet.*

## Installation

### NuGet

Using the NuGet package manager console:

```
Install-Package Tz.Net
```

[https://www.nuget.org/packages/Tz.Net/](https://www.nuget.org/packages/Tz.Net/ "https://www.nuget.org/packages/Tz.Net/")

## Usage

```cs
using Tz.Net;

namespace Tz.Net.Demo
{
	public class Foo
	{
		public async void Bar()
		{
			// Seed a wallet with mnemonic, email, and password.
			var wallet = new Wallet(15WordMnemonic, Email, Password);
	
			// Activates a wallet.
			var activationResult = await wallet.Activate("<your activation code>");
			
			// Get the wallet balance.
			var balance = await wallet.GetBalance();
	
			// Send 2 tezzies from sending "from" address to recipient "to" address.
			var transferOpResult = await wallet.Transfer(from: "tz1...", to: "tz1...", amount: 2, fee: 0);
		}
	}
}
```

See the tests project for more examples.

**NOTE:** The default connection is made to a locally running node at `http://localhost:8732`. Whatever node version is listening there will be on what network these calls are made (e.g. alphanet/zeronet/betanet).

## Testing

1. In command line, navigate to `tests\Tz.Net.Tests` directory
2. Execute `dotnet test`

## Wishlist

1. More features
2. More tests -- always
3. Wiki documentation
4. Contracts (Michelson => Micheline support would be super rad)
5. Help

## Contributing

I want help! Please don't hesitate to contribute. This project is being developed completely in spare time out of sheer admiration for Tezos and the expansion of the Tezos community. Pull requests will be reviewed as soon as possible.

If you introduce new functionality, if at all possible, please find a way to create unit tests that also test this new functionality (and ensure the changes do not break existing functionality by running the tests--see `Testing` section above).

Please do all new development on the `develop` branch and do pull requests from there.

## Issues

Please file any bugs you discover or feature requests you'd like to see.

[project-issues]: https://github.com/mscappini/tezos.net/issues

## Credits

I have to give special credit to [stephenandrews](https://github.com/stephenandrews) for creating [eztz](https://github.com/stephenandrews/eztz) and [LMilfont](https://github.com/LMilfont) for creating [TezosJ SDK](https://github.com/LMilfont/TezosJ). Their projects were invaluable resources for me as reference material to better understand the Tezos node RPC API. This project was only made possible because I stand on the shoulders of giants.

- [https://github.com/stephenandrews/eztz](https://github.com/stephenandrews/eztz) (reference material)
- [https://github.com/LMilfont/TezosJ](https://github.com/LMilfont/TezosJ) (reference material)
- [https://github.com/bitcoinjs/bip39](https://github.com/bitcoinjs/bip39) (mnemonic code support)
	- I translated this to C#.
- [https://github.com/kmaragon/Konscious.Security.Cryptography](https://github.com/kmaragon/Konscious.Security.Cryptography) (**Blake2B** generic hashing algorithm)
- [https://github.com/adamcaudill/Base58Check](https://github.com/adamcaudill/Base58Check) (**Base58 Checked Encoding** based on [public domain Gist](https://gist.github.com/CodesInChaos/3175971) by [CodesInChaos](https://github.com/codesinchaos))
	- I had to build and distribute a custom .NET core assembly for Tz.Net.
- [https://github.com/CodesInChaos/Chaos.NaCl](https://github.com/CodesInChaos/Chaos.NaCl) (**Ed25519** signature support)
	- I had to build and distribute a custom .NET core assembly for Tz.Net. 
- [https://github.com/Osinko/BigFloat](https://github.com/Osinko/BigFloat) (BigFloat support) 


## Author

Mark Scappini

## License

**Tz.Net** is available under the **MIT License**. Check the **LICENSE** file for details.