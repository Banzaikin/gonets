#!/bin/bash

PROJECT_DIR="/home/gonets"
LOGFILE="$PROJECT_DIR/monitor.log"
COMPOSE_FILE="$PROJECT_DIR/docker-compose.yml"

log() {
  echo "[$(date '+%Y-%m-%d %H:%M:%S')] $1" >> "$LOGFILE"
}

if ! command -v docker >/dev/null 2>&1; then
  log "Команда docker не найдена"
  exit 1
fi

# Проверим, работает ли docker
if ! systemctl is-active --quiet docker; then
  log "Docker не запущен, пробуем запустить..."
  systemctl start docker
  sleep 5
fi

# Перейдём в директорию проекта
cd "$PROJECT_DIR" || {
  log "Не удалось перейти в директорию $PROJECT_DIR"
  exit 1
}

# Проверим, работают ли контейнеры
REQUIRED_CONTAINERS=(postgres rabbitmq commonbackend clientbackend operatorbackend frontend)

for container in "${REQUIRED_CONTAINERS[@]}"; do
  if ! docker ps --format '{{.Names}}' | grep -Fxq "$container"; then
    log "Контейнер $container не запущен. Пробуем поднять его..."
    docker compose -f "$COMPOSE_FILE" up -d "$container"

    sleep 10

    if ! docker ps --format '{{.Names}}' | grep -Fxq "$container"; then
      log "Не удалось поднять контейнер $container. Перезапускаем весь проект..."
      docker compose -f "$COMPOSE_FILE" down
      sleep 5
      docker compose -f "$COMPOSE_FILE" up -d
      log "Полный перезапуск завершён."
      exit 0
    fi
  fi
done

# Проверка RabbitMQ UI
if ! curl -fsS http://localhost:15672 > /dev/null; then
  log "RabbitMQ UI недоступен. Перезапуск rabbitmq..."
  docker compose -f "$COMPOSE_FILE" restart rabbitmq
fi

# Проверка commonbackend - только если /health реально существует
if ! curl -fsS http://localhost:5000/health > /dev/null; then
  log "commonbackend /health недоступен. Перезапуск commonbackend..."
  docker compose -f "$COMPOSE_FILE" restart commonbackend
fi

# Проверка frontend
if ! curl -fsS http://localhost > /dev/null; then
  log "Frontend недоступен. Перезапуск frontend..."
  docker compose -f "$COMPOSE_FILE" restart frontend
fi

log "Все сервисы в норме."