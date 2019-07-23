# aras-to-uml
.NET Framework application that extracts a data model from Aras and converts it into a pseudo-UML .dot-file.

## Installation


### Pre-requisites

1. Admin access to an Aras Innovator instance is required to meaningfully run the 
   program

### Install Steps

1. Copy your IOM.dll into the References/IOM folder  
    i. You will need the IOM.dll from the CD image's "Utilities\Aras Innovator 12.0 IOM SDK" folder  
    ii. The IOM.dll from the Innovator/Server/bin of an Aras Innovator installation will not suffice
2. Build the solution in your IDE 

## Usage

1. Run the ArasToUml.exe with command line argument -h or --help to print out usage information
2. Once the program has finished and the .dot-file is created, use your Graphviz installation or equivalent online Graphviz tools to generate the pseudo-UML class diagram

## Contributing

1. Fork it!
2. Create your feature branch: `git checkout -b feature/feature-name`
3. Commit your changes: `git commit`  
  i. A meaningful commit subject and message are helpful: https://chris.beams.io/posts/git-commit/
4. Push to the branch: `git push origin feature/feature-name`
5. Submit a pull request

## Credits

Original code written by birdoxer.

## License

This project is published to Github under the MIT license. See the [LICENSE file](./LICENSE) for license rights and limitations.
