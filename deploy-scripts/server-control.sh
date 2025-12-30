#!/bin/bash

# Division1D Server Control Script (Podman)
# Oracle VM ARM64에서 사용할 Podman 컨테이너 기반 서버 관리 스크립트

SERVER_DIR=~/division1d
CONTAINER_NAME=unityds
LOG_FILE=~/division1d/server.log
SERVER_PORT=7777

# 색상 정의
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 함수: 서버 상태 확인
check_status() {
    if sudo podman ps | grep -q "$CONTAINER_NAME"; then
        echo -e "${GREEN}✅ Server container is running${NC}"
        sudo podman ps | grep "$CONTAINER_NAME"

        # 포트 확인
        if sudo ss -lunp | grep -q "$SERVER_PORT"; then
            echo -e "${GREEN}✅ Server is listening on UDP $SERVER_PORT${NC}"
        else
            echo -e "${YELLOW}⚠️  Port $SERVER_PORT not detected (initializing?)${NC}"
        fi
        return 0
    else
        echo -e "${RED}⛔ Server container is not running${NC}"

        # 중지된 컨테이너 확인
        if sudo podman ps -a | grep -q "$CONTAINER_NAME"; then
            echo -e "${YELLOW}⚠️  Container exists but is stopped${NC}"
            sudo podman ps -a | grep "$CONTAINER_NAME"
        fi
        return 1
    fi
}

# 함수: 서버 시작
start_server() {
    if check_status > /dev/null 2>&1; then
        echo -e "${YELLOW}⚠️  Server is already running${NC}"
        check_status
        return 1
    fi

    echo -e "${BLUE}🚀 Starting Division1D Server in Podman...${NC}"

    if [ ! -d "$SERVER_DIR" ]; then
        echo -e "${RED}❌ Server directory not found: $SERVER_DIR${NC}"
        return 1
    fi

    # 실행 파일 찾기
    cd "$SERVER_DIR"
    EXEC_FILE=$(find . -maxdepth 1 -type f -executable ! -name "*.so" ! -name "*.log" | head -1)

    if [ -z "$EXEC_FILE" ]; then
        echo -e "${RED}❌ No executable file found in $SERVER_DIR${NC}"
        ls -la "$SERVER_DIR"
        return 1
    fi

    EXEC_NAME=$(basename "$EXEC_FILE")
    echo -e "${BLUE}🎯 Found executable: $EXEC_NAME${NC}"

    # 실행 권한 확인
    chmod +x "$EXEC_NAME"

    # 기존 중지된 컨테이너 삭제
    sudo podman rm -f "$CONTAINER_NAME" 2>/dev/null || true

    # Podman 컨테이너 시작
    echo -e "${BLUE}🐳 Starting Podman container...${NC}"
    sudo podman run -d --name "$CONTAINER_NAME" \
        --network host \
        -v "$SERVER_DIR:/app:Z" \
        ubuntu:22.04 \
        bash -lc "cd /app && exec ./$EXEC_NAME -port $SERVER_PORT -logFile /app/server.log"

    sleep 3

    if check_status; then
        echo -e "${GREEN}✅ Server started successfully${NC}"
        echo -e "${BLUE}📊 Log file: $LOG_FILE${NC}"
        return 0
    else
        echo -e "${RED}❌ Failed to start server. Container logs:${NC}"
        sudo podman logs "$CONTAINER_NAME" || true
        echo -e "${RED}Server logs:${NC}"
        tail -n 20 "$LOG_FILE" 2>/dev/null || true
        return 1
    fi
}

# 함수: 서버 중지
stop_server() {
    echo -e "${YELLOW}🛑 Stopping Division1D Server...${NC}"

    if ! sudo podman ps | grep -q "$CONTAINER_NAME"; then
        echo -e "${YELLOW}⚠️  Server container is not running${NC}"

        # 중지된 컨테이너 정리
        if sudo podman ps -a | grep -q "$CONTAINER_NAME"; then
            echo -e "${BLUE}Removing stopped container...${NC}"
            sudo podman rm -f "$CONTAINER_NAME" 2>/dev/null || true
        fi
        return 0
    fi

    # Graceful shutdown 시도 (podman stop)
    echo -e "${BLUE}Attempting graceful shutdown...${NC}"
    sudo podman stop -t 10 "$CONTAINER_NAME" 2>/dev/null || true

    sleep 2

    # 강제 종료
    echo -e "${YELLOW}Removing container...${NC}"
    sudo podman rm -f "$CONTAINER_NAME" 2>/dev/null || true

    echo -e "${GREEN}✅ Server container stopped${NC}"
}

# 함수: 서버 재시작
restart_server() {
    echo -e "${BLUE}🔄 Restarting Division1D Server...${NC}"
    stop_server
    sleep 2
    start_server
}

# 함수: 로그 보기
show_logs() {
    if [ ! -f "$LOG_FILE" ]; then
        echo -e "${RED}❌ Log file not found: $LOG_FILE${NC}"
        return 1
    fi

    echo -e "${BLUE}📊 Server Logs (last 50 lines):${NC}"
    echo -e "${YELLOW}════════════════════════════════════════${NC}"
    tail -n 50 "$LOG_FILE"
    echo -e "${YELLOW}════════════════════════════════════════${NC}"
    echo -e "${BLUE}💡 Use 'tail -f $LOG_FILE' to follow live logs${NC}"
}

# 함수: 실시간 로그
follow_logs() {
    if [ ! -f "$LOG_FILE" ]; then
        echo -e "${RED}❌ Log file not found: $LOG_FILE${NC}"
        return 1
    fi

    echo -e "${BLUE}📊 Following server logs (Ctrl+C to stop)...${NC}"
    tail -f "$LOG_FILE"
}

# 함수: 사용법 출력
show_usage() {
    echo "════════════════════════════════════════"
    echo "Division1D Server Control Script"
    echo "════════════════════════════════════════"
    echo "Usage: $0 {start|stop|restart|status|logs|follow}"
    echo ""
    echo "Commands:"
    echo "  start   - Start the server"
    echo "  stop    - Stop the server"
    echo "  restart - Restart the server"
    echo "  status  - Check server status"
    echo "  logs    - Show recent logs"
    echo "  follow  - Follow live logs (Ctrl+C to stop)"
    echo "════════════════════════════════════════"
}

# 메인 로직
case "$1" in
    start)
        start_server
        ;;
    stop)
        stop_server
        ;;
    restart)
        restart_server
        ;;
    status)
        check_status
        ;;
    logs)
        show_logs
        ;;
    follow)
        follow_logs
        ;;
    *)
        show_usage
        exit 1
        ;;
esac

exit $?
