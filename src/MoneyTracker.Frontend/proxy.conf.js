const target = process.env.MONEYTRACKER_API_URL ?? 'http://localhost:5001';

module.exports = {
  '/api': {
    target,
    secure: false,
    changeOrigin: true,
    logLevel: 'warn',
  },
};
