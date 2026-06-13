const { Pool } = require('pg');

const dbConfig = {
  user: process.env.DB_USER,
  host: process.env.DB_HOST,
  database: process.env.DB_NAME,
  password: process.env.DB_PASSWORD,
  port: Number(process.env.DB_PORT) || 5432,
};

console.log('Postgres config loaded:', {
  user: dbConfig.user,
  host: dbConfig.host,
  database: dbConfig.database,
  port: dbConfig.port,
});

const pool = new Pool(dbConfig);

pool.on('error', (err) => {
  console.error('Unexpected Postgres pool error:', err);
});

module.exports = pool;