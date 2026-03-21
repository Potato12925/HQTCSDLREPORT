/** @type {import('tailwindcss').Config} */
export default {
    content: ["./index.html", "./src/**/*.{vue,js,ts}"],
    theme: {
        extend: {
            colors: {
                primary: '#C08552',   // màu chính (nâu cam)
                secondary: '#8C5A3C', // nâu trung
                dark: '#4B2E2B',      // nâu đậm
                light: '#FFF8F0',     // nền sáng
            }
        },
    },
    plugins: [],
}