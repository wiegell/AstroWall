#!/bin/bash
pkgbuild --root ./bin/Release/Astro\ Wall.app --scripts ./bin/PackageScripts --identifier com.astro.wall.Astro-Wall --install-location /Applications/Astro\ Wall.app ./bin/Package/com.astro.wall.Astro-Wall.pkg
productbuild --distribution bin/Package/distribution.dist --product obj/Release/Product.plist  --package-path ./bin/Package/ --resources ./bin/Resources/  ./bin/Package/Astro.pkg