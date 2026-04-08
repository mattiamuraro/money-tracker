const target = process.env.MONEYTRACKER_API_URL ?? 'https://localhost:7088';

module.exports = {
  '/api': {
    target,
    secure: false,
    changeOrigin: true,
    logLevel: 'warn',
  },
};
