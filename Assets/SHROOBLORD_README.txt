Now nothing of either RaymapGame or R2Shroo exist within the RayMap folder structure anymore physically. Instead, they're symlinked to projects that need to be based in the same directory as this project.

I.e. the symlinks specifically look for ../R2Shroo/bin/ for example, when looking for the /bin/ folder from the directory where this README is located.