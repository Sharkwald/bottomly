docker stop bottomly
docker build --tag bottomly -f run.dockerfile .
docker system prune -f
docker run --rm --name bottomly --env-file .env bottomly