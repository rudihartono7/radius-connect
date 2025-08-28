-- FreeRADIUS MySQL Database Schema
-- This script creates the standard FreeRADIUS tables

-- Create the radius database if it doesn't exist
CREATE DATABASE IF NOT EXISTS `radius`;
USE `radius`;

-- Standard FreeRADIUS tables
CREATE TABLE IF NOT EXISTS `radcheck` (
  `id` int(11) unsigned NOT NULL auto_increment,
  `username` varchar(64) NOT NULL default '',
  `attribute` varchar(64) NOT NULL default '',
  `op` char(2) NOT NULL DEFAULT '==',
  `value` varchar(253) NOT NULL default '',
  PRIMARY KEY (`id`),
  KEY `username` (`username`(32))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `radreply` (
  `id` int(11) unsigned NOT NULL auto_increment,
  `username` varchar(64) NOT NULL default '',
  `attribute` varchar(64) NOT NULL default '',
  `op` char(2) NOT NULL DEFAULT '=',
  `value` varchar(253) NOT NULL default '',
  PRIMARY KEY (`id`),
  KEY `username` (`username`(32))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `radusergroup` (
  `username` varchar(64) NOT NULL default '',
  `groupname` varchar(64) NOT NULL default '',
  `priority` int(11) NOT NULL default '1',
  KEY `username` (`username`(32))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `radgroupcheck` (
  `id` int(11) unsigned NOT NULL auto_increment,
  `groupname` varchar(64) NOT NULL default '',
  `attribute` varchar(64) NOT NULL default '',
  `op` char(2) NOT NULL DEFAULT '==',
  `value` varchar(253) NOT NULL default '',
  PRIMARY KEY (`id`),
  KEY `groupname` (`groupname`(32))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `radgroupreply` (
  `id` int(11) unsigned NOT NULL auto_increment,
  `groupname` varchar(64) NOT NULL default '',
  `attribute` varchar(64) NOT NULL default '',
  `op` char(2) NOT NULL DEFAULT '=',
  `value` varchar(253) NOT NULL default '',
  PRIMARY KEY (`id`),
  KEY `groupname` (`groupname`(32))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `radacct` (
  `radacctid` bigint(21) NOT NULL auto_increment,
  `acctsessionid` varchar(64) NOT NULL default '',
  `acctuniqueid` varchar(32) NOT NULL default '',
  `username` varchar(64) NOT NULL default '',
  `groupname` varchar(64) NOT NULL default '',
  `realm` varchar(64) default '',
  `nasipaddress` varchar(15) NOT NULL default '',
  `nasportid` varchar(15) default NULL,
  `nasporttype` varchar(32) default NULL,
  `acctstarttime` datetime NULL default NULL,
  `acctupdatetime` datetime NULL default NULL,
  `acctstoptime` datetime NULL default NULL,
  `acctinterval` int(12) default NULL,
  `acctsessiontime` int(12) unsigned default NULL,
  `acctauthentic` varchar(32) default NULL,
  `connectinfo_start` varchar(50) default NULL,
  `connectinfo_stop` varchar(50) default NULL,
  `acctinputoctets` bigint(20) default NULL,
  `acctoutputoctets` bigint(20) default NULL,
  `calledstationid` varchar(50) NOT NULL default '',
  `callingstationid` varchar(50) NOT NULL default '',
  `acctterminatecause` varchar(32) NOT NULL default '',
  `servicetype` varchar(32) default NULL,
  `framedprotocol` varchar(32) default NULL,
  `framedipaddress` varchar(15) NOT NULL default '',
  PRIMARY KEY (`radacctid`),
  UNIQUE KEY `acctuniqueid` (`acctuniqueid`),
  KEY `username` (`username`),
  KEY `framedipaddress` (`framedipaddress`),
  KEY `acctsessionid` (`acctsessionid`),
  KEY `acctsessiontime` (`acctsessiontime`),
  KEY `acctstarttime` (`acctstarttime`),
  KEY `acctinterval` (`acctinterval`),
  KEY `acctstoptime` (`acctstoptime`),
  KEY `nasipaddress` (`nasipaddress`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `radpostauth` (
  `id` int(11) NOT NULL auto_increment,
  `username` varchar(64) NOT NULL default '',
  `pass` varchar(64) NOT NULL default '',
  `reply` varchar(32) NOT NULL default '',
  `authdate` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `username` (`username`),
  KEY `authdate` (`authdate`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `nas` (
  `id` int(10) NOT NULL auto_increment,
  `nasname` varchar(128) NOT NULL,
  `shortname` varchar(32),
  `type` varchar(30) DEFAULT 'other',
  `ports` int(5),
  `secret` varchar(60) DEFAULT 'secret',
  `server` varchar(64),
  `community` varchar(50),
  `description` varchar(200) DEFAULT 'RADIUS Client',
  PRIMARY KEY (`id`),
  KEY `nasname` (`nasname`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Performance indexes for FreeRADIUS tables
CREATE INDEX IF NOT EXISTS `idx_radacct_acctstoptime` ON `radacct` (`acctstoptime`);
CREATE INDEX IF NOT EXISTS `idx_radacct_acctstarttime` ON `radacct` (`acctstarttime`);
CREATE INDEX IF NOT EXISTS `idx_radacct_username_starttime` ON `radacct` (`username`, `acctstarttime`);
CREATE INDEX IF NOT EXISTS `idx_radacct_nasip_starttime` ON `radacct` (`nasipaddress`, `acctstarttime`);
CREATE INDEX IF NOT EXISTS `idx_radpostauth_authdate` ON `radpostauth` (`authdate`);
CREATE INDEX IF NOT EXISTS `idx_radpostauth_username_authdate` ON `radpostauth` (`username`, `authdate`);
CREATE INDEX IF NOT EXISTS `idx_radpostauth_reply_authdate` ON `radpostauth` (`reply`, `authdate`);

-- Insert some sample data for testing
INSERT IGNORE INTO `radcheck` (`username`, `attribute`, `op`, `value`) VALUES
('testuser', 'Cleartext-Password', ':=', 'testpass'),
('admin', 'Cleartext-Password', ':=', 'admin123');

INSERT IGNORE INTO `radusergroup` (`username`, `groupname`, `priority`) VALUES
('testuser', 'users', 1),
('admin', 'admins', 1);

INSERT IGNORE INTO `radgroupreply` (`groupname`, `attribute`, `op`, `value`) VALUES
('users', 'Session-Timeout', ':=', '3600'),
('admins', 'Session-Timeout', ':=', '7200');

-- Sample accounting data for testing
INSERT IGNORE INTO `radacct` (
  `acctsessionid`, `acctuniqueid`, `username`, `groupname`, `realm`, 
  `nasipaddress`, `acctstarttime`, `acctsessiontime`, 
  `acctinputoctets`, `acctoutputoctets`, `framedipaddress`
) VALUES
('test-session-1', 'unique-1', 'testuser', 'users', '', 
 '192.168.1.1', NOW() - INTERVAL 1 HOUR, 3600, 
 1048576, 2097152, '10.0.0.100'),
('test-session-2', 'unique-2', 'admin', 'admins', '', 
 '192.168.1.1', NOW() - INTERVAL 30 MINUTE, NULL, 
 524288, 1048576, '10.0.0.101');

-- Sample authentication logs
INSERT IGNORE INTO `radpostauth` (`username`, `pass`, `reply`, `authdate`) VALUES
('testuser', 'testpass', 'Access-Accept', NOW() - INTERVAL 1 HOUR),
('admin', 'admin123', 'Access-Accept', NOW() - INTERVAL 30 MINUTE),
('baduser', 'wrongpass', 'Access-Reject', NOW() - INTERVAL 15 MINUTE);

COMMIT;