const { Pool } = require('pg');

// Create pool from DATABASE_URL or individual config variables
const dbConfig = process.env.DATABASE_URL 
  ? {
      // Production: Use connection string (supports SSL automatically)
      connectionString: process.env.DATABASE_URL,
      ssl: process.env.NODE_ENV === 'production' 
        ? { rejectUnauthorized: false }  // Required for Supabase
        : false,
      max: 20,                            // Connection pool size
      idleTimeoutMillis: 30000,
      connectionTimeoutMillis: 2000,
    }
  : {
      // Local development: Use individual variables
      user: process.env.DB_USER || 'postgres',
      host: process.env.DB_HOST || 'localhost',
      database: process.env.DB_NAME || 'fleet_nexus',
      password: process.env.DB_PASSWORD,
      port: Number(process.env.DB_PORT) || 5432,
      max: 20,
      idleTimeoutMillis: 30000,
      connectionTimeoutMillis: 2000,
    };

console.log('Database config loaded:', {
  environment: process.env.NODE_ENV || 'development',
  connectionMethod: process.env.DATABASE_URL ? 'Connection String' : 'Individual Variables',
  host: dbConfig.connectionString ? 'supabase' : (dbConfig.host || 'localhost'),
});

const pool = new Pool(dbConfig);

pool.on('error', (err) => {
  console.error('Unexpected Postgres pool error:', err);
  process.exit(1);  // Exit on unrecoverable pool error
});

module.exports = pool;