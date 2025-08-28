CREATE TABLE ClientHostData (
    HostID BIGINT PRIMARY KEY,
    HostName VARCHAR(255) NOT NULL,
    HostIP VARCHAR(45) NOT NULL,
    ClientOS VARCHAR(255),
    ClientVersion VARCHAR(20),
    DateAdded TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
CREATE TABLE ClientHostDataMetrics (
    MetricID BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    HostID BIGINT NOT NULL,
    TTL double precision,
    DateAdded TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);



CREATE OR REPLACE PROCEDURE upsert_client_host_data(
    p_HostID BIGINT,
    p_HostName VARCHAR,
    p_HostIP VARCHAR,
    p_ClientOS VARCHAR,
    p_ClientVersion VARCHAR
)
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO ClientHostData (HostID, HostName, HostIP, ClientOS, ClientVersion, DateAdded)
    VALUES (p_HostID, p_HostName, p_HostIP, p_ClientOS, p_ClientVersion, CURRENT_TIMESTAMP)
    ON CONFLICT (HostID) DO UPDATE
    SET HostName      = EXCLUDED.HostName,
        HostIP        = EXCLUDED.HostIP,
        ClientOS      = EXCLUDED.ClientOS,
        ClientVersion = EXCLUDED.ClientVersion,
        DateAdded     = CURRENT_TIMESTAMP;
END;
$$;
