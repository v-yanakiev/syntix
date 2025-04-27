const dev = {
  apiUrl: 'http://localhost:8080/'
}

const prod = {
  apiUrl: '/api/'
}
//export const config = prod
export const apiConfig = import.meta.env.PROD ? prod : dev