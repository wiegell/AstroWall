#!/bin/bash
pkgbuild --root ./bin/Release/Astro\ Wall.app --scripts ./pkgsrc/scripts --identifier com.astro.wall.Astro-Wall --install-location /Applications/Astro\ Wall.app ./bin/Package/com.astro.wall.Astro-Wall.pkg
productbuild --distribution ./pkgsrc/distribution.dist --product pkgsrc/Product.plist --package-path ./bin/Package/ --resources ./pkgsrc/resources/  ./bin/Package/Astro.pkg