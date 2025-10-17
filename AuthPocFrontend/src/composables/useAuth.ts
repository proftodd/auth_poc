import { ref } from 'vue'

export type User = {
    Name?: string;
    Login: string;
    Url: string;
    SiteAdmin: boolean;
    Company?: string;
    OrganizationUrl?: string;
    Jwt: string;
}

const user = ref<User | null>(null)
// const router = useRouter()

function parseJwt(token: string) {
    try {
        const payload = token.split('.')[1]
        return JSON.parse(atob(payload))
    } catch {
        return null
    }
}

function isTokenExpired(token: string, bufferSeconds = 30) {
    const payload = parseJwt(token)
    if (!payload?.exp) {
        return true
    }
    const now = Math.floor(Date.now() / 1000)
    return payload.exp < now + bufferSeconds
}

export function useAuth() {
    const init = (queryToken?: string) => {
        if (queryToken) {
            user.value = JSON.parse(queryToken) as User
            localStorage.setItem('authedUser', queryToken)
        } else {
            const saved = localStorage.getItem('authedUser')
            if (saved) {
                user.value = JSON.parse(saved) as User
            }
        }
    }

    const isLoggedIn = () => !!user.value?.Jwt

    const refreshToken = async(): Promise<boolean> => {
        if (!user.value) {
            return false
        }
        try {
            const res = await fetch('https://localhost:7063/auth/jwt', {
                headers: { Authorization: `Bearer ${user.value.Jwt}` },
            })
            const data = await res.json()
            if (data.access_token) {
                user.value.Jwt = data.access_token
                localStorage.setItem('authedUser', JSON.stringify(user.value))
                return true
            } else {
                logout()
                return false
            }
        } catch {
            logout()
            return false
        }
    }

    const refreshTokenIfNeeded = async(): Promise<boolean> => {
        if (!user.value) {
            return false
        }
        return isTokenExpired(user.value.Jwt)
            ? await refreshToken()
            : true
    }

    const logout = () => {
        user.value = null
        localStorage.removeItem('authedUser')
    }

    return { user, init, isLoggedIn, refreshToken, refreshTokenIfNeeded, logout }
}
