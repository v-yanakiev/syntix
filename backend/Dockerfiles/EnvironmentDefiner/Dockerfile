FROM debian:bookworm-slim

RUN apt update && apt install -y curl && \
    curl -L https://fly.io/install.sh | sh && \
    apt remove -y curl

