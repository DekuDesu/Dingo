# How to use Dingo SQL Importing

`AutoImport.sh` will attempt to import all `.sql` scripts contained within the `SqlToImport` folder. The entire contents of `./SQL` is mirrored to `/var/opt/mssql/` when the container starts. This can alternatively be done in a docker file using `COPY` or in-container using `curl` should you wish.
