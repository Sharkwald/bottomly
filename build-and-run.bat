docker build --tag docker-bottomly .
docker system prune -f
docker run --rm --name bottomly --env-file .env docker-bottomly