﻿using System.IO;

namespace SA3D.SA2CutscenePlayer.Media
{
    public struct VoiceEntry
    {
        public static VoiceEntry[] Default => new VoiceEntry[]
        {
            new(false, 0000, 10, 0),
            new(false, 0000, 20, 1),
            new(false, 0000, 30, 2),
            new(false, 0000, 40, 3),
            new(false, 0000, 50, 4),
            new(false, 0000, 60, 5),
            new(false, 0000, 70, 6),
            new(false, 0000, 80, 7),
            new(false, 0000, 90, 8),
            new(false, 0000, 100, 9),
            new(true, 0001, 10, 10),
            new(true, 0001, 20, 11),
            new(false, 0002, 10, 12),
            new(false, 0002, 20, 13),
            new(false, 0002, 30, 14),
            new(false, 0002, 40, 15),
            new(false, 0002, 50, 16),
            new(false, 0002, 60, 17),
            new(false, 0002, 61, 18),
            new(false, 0002, 62, 19),
            new(false, 0002, 63, 20),
            new(false, 0002, 80, 21),
            new(false, 0002, 110, 22),
            new(false, 0002, 130, 23),
            new(false, 0002, 140, 24),
            new(false, 0002, 141, 25),
            new(false, 0002, 142, 26),
            new(false, 0002, 150, 27),
            new(false, 0002, 160, 28),
            new(false, 0002, 170, 29),
            new(false, 0002, 180, 30),
            new(false, 0002, 191, 31),
            new(false, 0002, 192, 32),
            new(false, 0003, 10, 33),
            new(false, 0003, 20, 34),
            new(false, 0003, 21, 35),
            new(false, 0003, 22, 36),
            new(false, 0003, 30, 37),
            new(false, 0003, 40, 38),
            new(false, 0003, 50, 39),
            new(false, 0003, 60, 40),
            new(false, 0003, 70, 41),
            new(false, 0003, 80, 42),
            new(false, 0003, 90, 43),
            new(false, 0003, 100, 44),
            new(false, 0003, 111, 45),
            new(false, 0003, 112, 46),
            new(false, 0003, 120, 47),
            new(false, 0003, 130, 48),
            new(false, 0003, 140, 49),
            new(false, 0003, 150, 50),
            new(false, 0003, 160, 51),
            new(false, 0004, 12, 52),
            new(false, 0004, 22, 53),
            new(false, 0004, 32, 54),
            new(false, 0004, 42, 55),
            new(false, 0004, 52, 56),
            new(false, 0005, 10, 57),
            new(false, 0005, 20, 58),
            new(false, 0005, 30, 59),
            new(false, 0005, 40, 60),
            new(false, 0005, 50, 61),
            new(false, 0005, 60, 62),
            new(false, 0005, 70, 63),
            new(false, 0005, 80, 64),
            new(false, 0006, 10, 65),
            new(false, 0006, 20, 66),
            new(false, 0006, 30, 67),
            new(false, 0006, 40, 68),
            new(false, 0006, 50, 69),
            new(false, 0006, 60, 70),
            new(false, 0006, 70, 71),
            new(false, 0006, 80, 72),
            new(false, 0006, 90, 73),
            new(false, 0006, 100, 74),
            new(false, 0006, 110, 75),
            new(false, 0006, 120, 76),
            new(false, 0006, 130, 77),
            new(false, 0006, 140, 78),
            new(false, 0006, 150, 79),
            new(true, 0007, 10, 80),
            new(true, 0007, 20, 81),
            new(true, 0007, 30, 82),
            new(true, 0007, 40, 83),
            new(true, 0007, 50, 84),
            new(true, 0007, 60, 85),
            new(true, 0008, 10, 86),
            new(true, 0008, 20, 87),
            new(true, 0008, 30, 88),
            new(true, 0010, 10, 89),
            new(false, 0011, 10, 90),
            new(false, 0011, 20, 91),
            new(false, 0011, 30, 92),
            new(false, 0011, 40, 93),
            new(false, 0011, 50, 94),
            new(false, 0011, 60, 95),
            new(false, 0011, 70, 96),
            new(false, 0011, 80, 97),
            new(false, 0011, 90, 98),
            new(false, 0011, 100, 99),
            new(false, 0011, 101, 100),
            new(false, 0011, 102, 101),
            new(false, 0011, 110, 102),
            new(false, 0011, 120, 103),
            new(false, 0011, 130, 104),
            new(false, 0011, 140, 105),
            new(true, 0012, 10, 106),
            new(true, 0012, 20, 107),
            new(true, 0012, 30, 108),
            new(true, 0012, 40, 109),
            new(true, 0012, 50, 110),
            new(true, 0012, 60, 111),
            new(true, 0012, 70, 112),
            new(true, 0012, 80, 113),
            new(true, 0013, 10, 114),
            new(false, 0014, 10, 115),
            new(false, 0014, 20, 116),
            new(false, 0014, 30, 117),
            new(false, 0014, 40, 118),
            new(false, 0014, 50, 119),
            new(false, 0014, 60, 120),
            new(false, 0014, 70, 121),
            new(false, 0014, 80, 122),
            new(false, 0014, 90, 123),
            new(false, 0014, 100, 124),
            new(false, 0014, 110, 125),
            new(false, 0014, 120, 126),
            new(false, 0014, 130, 127),
            new(false, 0015, 10, 128),
            new(false, 0015, 20, 129),
            new(false, 0015, 30, 130),
            new(false, 0015, 40, 131),
            new(false, 0015, 50, 132),
            new(false, 0015, 60, 133),
            new(false, 0015, 70, 134),
            new(false, 0015, 80, 135),
            new(false, 0015, 90, 136),
            new(false, 0015, 100, 137),
            new(false, 0015, 110, 138),
            new(false, 0015, 120, 139),
            new(false, 0015, 130, 140),
            new(false, 0015, 131, 141),
            new(false, 0015, 132, 142),
            new(false, 0015, 140, 143),
            new(false, 0015, 150, 144),
            new(false, 0015, 160, 145),
            new(false, 0015, 170, 146),
            new(false, 0015, 180, 147),
            new(false, 0016, 10, 148),
            new(false, 0016, 20, 149),
            new(false, 0016, 30, 150),
            new(false, 0016, 40, 151),
            new(false, 0016, 50, 152),
            new(false, 0016, 60, 153),
            new(false, 0017, 10, 154),
            new(false, 0017, 20, 155),
            new(false, 0017, 30, 156),
            new(false, 0017, 40, 157),
            new(false, 0017, 50, 158),
            new(false, 0017, 60, 159),
            new(true, 0018, 10, 160),
            new(true, 0018, 20, 161),
            new(true, 0018, 22, 162),
            new(false, 0019, 10, 163),
            new(false, 0019, 20, 164),
            new(false, 0019, 30, 165),
            new(false, 0019, 40, 166),
            new(false, 0019, 50, 167),
            new(false, 0019, 60, 168),
            new(false, 0019, 70, 169),
            new(false, 0019, 80, 170),
            new(false, 0019, 90, 171),
            new(false, 0019, 100, 172),
            new(false, 0019, 110, 173),
            new(false, 0020, 10, 174),
            new(false, 0020, 20, 175),
            new(false, 0020, 30, 176),
            new(false, 0020, 41, 177),
            new(false, 0020, 42, 178),
            new(false, 0020, 50, 179),
            new(false, 0020, 60, 180),
            new(false, 0020, 70, 181),
            new(false, 0020, 80, 182),
            new(false, 0020, 90, 183),
            new(false, 0020, 100, 184),
            new(false, 0020, 110, 185),
            new(false, 0020, 120, 186),
            new(false, 0021, 10, 187),
            new(false, 0021, 20, 188),
            new(false, 0021, 30, 189),
            new(false, 0021, 40, 190),
            new(false, 0021, 50, 191),
            new(false, 0021, 60, 192),
            new(false, 0021, 61, 193),
            new(false, 0021, 62, 194),
            new(false, 0021, 70, 195),
            new(false, 0021, 80, 196),
            new(false, 0021, 90, 197),
            new(false, 0021, 100, 198),
            new(false, 0021, 110, 199),
            new(false, 0021, 120, 200),
            new(false, 0021, 130, 201),
            new(false, 0021, 140, 202),
            new(false, 0021, 150, 203),
            new(false, 0021, 160, 204),
            new(false, 0022, 10, 205),
            new(false, 0022, 20, 206),
            new(false, 0022, 30, 207),
            new(false, 0022, 40, 208),
            new(false, 0022, 50, 209),
            new(false, 0022, 60, 210),
            new(true, 0023, 10, 211),
            new(true, 0023, 20, 212),
            new(false, 0024, 10, 213),
            new(false, 0024, 20, 214),
            new(false, 0024, 30, 215),
            new(false, 0024, 40, 216),
            new(false, 0024, 50, 217),
            new(false, 0024, 60, 218),
            new(false, 0024, 70, 219),
            new(false, 0024, 80, 220),
            new(false, 0024, 90, 221),
            new(false, 0024, 100, 222),
            new(false, 0024, 110, 223),
            new(false, 0024, 120, 224),
            new(false, 0024, 130, 225),
            new(false, 0024, 140, 226),
            new(false, 0024, 150, 227),
            new(false, 0024, 160, 228),
            new(false, 0024, 170, 229),
            new(false, 0024, 180, 230),
            new(false, 0024, 190, 231),
            new(false, 0024, 200, 232),
            new(false, 0024, 210, 233),
            new(false, 0025, 10, 234),
            new(false, 0025, 50, 235),
            new(false, 0025, 60, 236),
            new(false, 0025, 70, 237),
            new(false, 0025, 90, 238),
            new(false, 0025, 100, 239),
            new(false, 0025, 110, 240),
            new(false, 0025, 120, 241),
            new(false, 0025, 141, 242),
            new(false, 0025, 142, 243),
            new(false, 0025, 150, 244),
            new(false, 0025, 160, 245),
            new(false, 0025, 170, 246),
            new(false, 0025, 190, 247),
            new(false, 0025, 200, 248),
            new(false, 0025, 210, 249),
            new(false, 0025, 220, 250),
            new(false, 0025, 230, 251),
            new(false, 0025, 240, 252),
            new(false, 0025, 250, 253),
            new(false, 0025, 260, 254),
            new(false, 0025, 270, 255),
            new(false, 0025, 280, 256),
            new(false, 0025, 290, 257),
            new(false, 0025, 300, 258),
            new(false, 0026, 10, 259),
            new(false, 0026, 20, 260),
            new(false, 0026, 30, 261),
            new(false, 0026, 40, 262),
            new(false, 0026, 50, 263),
            new(false, 0026, 60, 264),
            new(false, 0026, 70, 265),
            new(false, 0027, 10, 266),
            new(false, 0027, 20, 267),
            new(false, 0027, 30, 268),
            new(false, 0027, 40, 269),
            new(false, 0027, 50, 270),
            new(false, 0027, 60, 271),
            new(false, 0027, 62, 272),
            new(false, 0027, 70, 273),
            new(false, 0028, 10, 274),
            new(false, 0028, 20, 275),
            new(false, 0028, 30, 276),
            new(false, 0028, 40, 277),
            new(false, 0028, 50, 278),
            new(false, 0028, 60, 279),
            new(false, 0029, 10, 280),
            new(false, 0029, 20, 281),
            new(false, 0029, 30, 282),
            new(false, 0030, 10, 283),
            new(false, 0030, 20, 284),
            new(false, 0030, 30, 285),
            new(false, 0030, 40, 286),
            new(false, 0030, 50, 287),
            new(false, 0030, 60, 288),
            new(false, 0030, 70, 289),
            new(false, 0030, 80, 290),
            new(false, 0030, 90, 291),
            new(false, 0030, 100, 292),
            new(false, 0030, 110, 293),
            new(false, 0030, 120, 294),
            new(false, 0030, 130, 295),
            new(false, 0030, 140, 296),
            new(false, 0030, 150, 297),
            new(false, 0030, 160, 298),
            new(false, 0030, 170, 299),
            new(false, 0030, 180, 300),
            new(false, 0030, 190, 301),
            new(false, 0030, 200, 302),
            new(false, 0030, 210, 303),
            new(false, 0030, 220, 304),
            new(false, 0100, 10, 305),
            new(false, 0100, 20, 306),
            new(false, 0100, 30, 307),
            new(false, 0101, 10, 308),
            new(false, 0101, 20, 309),
            new(false, 0101, 30, 310),
            new(false, 0101, 40, 311),
            new(false, 0101, 50, 312),
            new(false, 0101, 60, 313),
            new(false, 0101, 70, 314),
            new(false, 0101, 72, 315),
            new(false, 0101, 80, 316),
            new(false, 0101, 82, 317),
            new(false, 0101, 90, 318),
            new(false, 0101, 100, 319),
            new(false, 0101, 102, 320),
            new(false, 0102, 10, 321),
            new(false, 0102, 20, 322),
            new(false, 0102, 30, 323),
            new(false, 0102, 40, 324),
            new(false, 0102, 50, 325),
            new(false, 0102, 60, 326),
            new(false, 0103, 10, 327),
            new(false, 0103, 20, 328),
            new(false, 0103, 21, 329),
            new(false, 0103, 22, 330),
            new(false, 0103, 30, 331),
            new(false, 0103, 40, 332),
            new(false, 0103, 50, 333),
            new(false, 0103, 60, 334),
            new(false, 0103, 70, 335),
            new(false, 0103, 80, 336),
            new(false, 0103, 90, 337),
            new(false, 0103, 100, 338),
            new(false, 0103, 111, 339),
            new(false, 0103, 112, 340),
            new(false, 0103, 120, 341),
            new(false, 0103, 130, 342),
            new(false, 0103, 140, 343),
            new(false, 0103, 150, 344),
            new(false, 0103, 160, 345),
            new(true, 0104, 10, 346),
            new(false, 0105, 10, 347),
            new(false, 0105, 20, 348),
            new(false, 0105, 30, 349),
            new(false, 0105, 40, 350),
            new(false, 0105, 50, 351),
            new(false, 0106, 10, 352),
            new(false, 0106, 20, 353),
            new(false, 0106, 30, 354),
            new(false, 0106, 40, 355),
            new(false, 0106, 60, 356),
            new(false, 0106, 70, 357),
            new(false, 0106, 80, 358),
            new(false, 0106, 90, 359),
            new(false, 0106, 100, 360),
            new(false, 0106, 120, 361),
            new(false, 0106, 130, 362),
            new(false, 0107, 10, 363),
            new(false, 0107, 20, 364),
            new(false, 0107, 30, 365),
            new(false, 0107, 40, 366),
            new(false, 0107, 50, 367),
            new(false, 0107, 60, 368),
            new(false, 0107, 61, 369),
            new(false, 0107, 62, 370),
            new(false, 0107, 63, 371),
            new(false, 0107, 80, 372),
            new(false, 0107, 110, 373),
            new(false, 0107, 130, 374),
            new(false, 0107, 140, 375),
            new(false, 0107, 141, 376),
            new(false, 0107, 142, 377),
            new(false, 0107, 150, 378),
            new(false, 0107, 160, 379),
            new(false, 0107, 170, 380),
            new(false, 0107, 180, 381),
            new(false, 0107, 190, 382),
            new(false, 0107, 191, 383),
            new(false, 0107, 192, 384),
            new(true, 0108, 10, 385),
            new(true, 0108, 20, 386),
            new(false, 0109, 10, 387),
            new(true, 0110, 10, 388),
            new(false, 0111, 10, 389),
            new(false, 0111, 20, 390),
            new(false, 0111, 31, 391),
            new(false, 0111, 32, 392),
            new(false, 0111, 41, 393),
            new(false, 0111, 42, 394),
            new(false, 0111, 50, 395),
            new(false, 0111, 60, 396),
            new(false, 0111, 70, 397),
            new(false, 0111, 80, 398),
            new(false, 0111, 90, 399),
            new(false, 0111, 100, 400),
            new(false, 0111, 110, 401),
            new(false, 0111, 120, 402),
            new(false, 0111, 130, 403),
            new(false, 0111, 140, 404),
            new(false, 0111, 150, 405),
            new(false, 0111, 160, 406),
            new(false, 0111, 170, 407),
            new(false, 0111, 180, 408),
            new(false, 0111, 191, 409),
            new(false, 0111, 192, 410),
            new(false, 0112, 10, 411),
            new(false, 0112, 20, 412),
            new(false, 0112, 30, 413),
            new(false, 0112, 40, 414),
            new(false, 0112, 50, 415),
            new(false, 0112, 60, 416),
            new(false, 0113, 10, 417),
            new(false, 0113, 20, 418),
            new(false, 0113, 30, 419),
            new(false, 0113, 40, 420),
            new(false, 0113, 50, 421),
            new(false, 0113, 60, 422),
            new(false, 0113, 70, 423),
            new(false, 0113, 80, 424),
            new(false, 0113, 90, 425),
            new(true, 0114, 10, 426),
            new(true, 0114, 20, 427),
            new(true, 0114, 30, 428),
            new(true, 0115, 10, 429),
            new(false, 0116, 10, 430),
            new(false, 0116, 20, 431),
            new(false, 0116, 30, 432),
            new(false, 0116, 40, 433),
            new(false, 0116, 50, 434),
            new(true, 0117, 10, 435),
            new(true, 0117, 20, 436),
            new(true, 0117, 30, 437),
            new(true, 0117, 40, 438),
            new(true, 0117, 50, 439),
            new(true, 0117, 60, 440),
            new(false, 0118, 10, 441),
            new(false, 0118, 20, 442),
            new(false, 0118, 30, 443),
            new(false, 0119, 10, 444),
            new(false, 0119, 12, 445),
            new(false, 0119, 20, 446),
            new(false, 0119, 30, 447),
            new(false, 0119, 40, 448),
            new(false, 0119, 50, 449),
            new(false, 0119, 60, 450),
            new(false, 0119, 70, 451),
            new(false, 0119, 80, 452),
            new(false, 0119, 90, 453),
            new(false, 0119, 100, 454),
            new(false, 0119, 110, 455),
            new(false, 0119, 120, 456),
            new(false, 0119, 130, 457),
            new(false, 0120, 10, 458),
            new(false, 0120, 20, 459),
            new(false, 0120, 30, 460),
            new(false, 0120, 40, 461),
            new(false, 0120, 50, 462),
            new(false, 0120, 60, 463),
            new(false, 0120, 70, 464),
            new(false, 0120, 80, 465),
            new(false, 0120, 90, 466),
            new(false, 0120, 100, 467),
            new(false, 0120, 110, 468),
            new(false, 0120, 120, 469),
            new(false, 0120, 122, 470),
            new(false, 0120, 130, 471),
            new(false, 0120, 140, 472),
            new(false, 0120, 150, 473),
            new(true, 0121, 10, 474),
            new(true, 0121, 20, 475),
            new(true, 0121, 30, 476),
            new(false, 0122, 10, 477),
            new(false, 0122, 20, 478),
            new(false, 0123, 10, 479),
            new(false, 0123, 20, 480),
            new(false, 0123, 30, 481),
            new(false, 0123, 40, 482),
            new(false, 0123, 50, 483),
            new(false, 0123, 60, 484),
            new(false, 0123, 70, 485),
            new(false, 0123, 80, 486),
            new(false, 0123, 90, 487),
            new(false, 0123, 100, 488),
            new(false, 0123, 110, 489),
            new(false, 0123, 130, 490),
            new(false, 0123, 140, 491),
            new(false, 0123, 150, 492),
            new(false, 0123, 160, 493),
            new(false, 0123, 170, 494),
            new(false, 0123, 180, 495),
            new(false, 0124, 10, 496),
            new(false, 0124, 20, 497),
            new(false, 0124, 30, 498),
            new(false, 0124, 40, 499),
            new(false, 0124, 50, 500),
            new(false, 0124, 60, 501),
            new(true, 0125, 10, 502),
            new(true, 0125, 20, 503),
            new(false, 0126, 10, 504),
            new(false, 0126, 20, 505),
            new(false, 0126, 30, 506),
            new(false, 0126, 40, 507),
            new(false, 0126, 50, 508),
            new(false, 0126, 60, 509),
            new(false, 0126, 70, 510),
            new(false, 0126, 80, 511),
            new(false, 0126, 90, 512),
            new(false, 0126, 100, 513),
            new(false, 0126, 110, 514),
            new(false, 0126, 120, 515),
            new(false, 0126, 130, 516),
            new(false, 0126, 140, 517),
            new(false, 0126, 150, 518),
            new(false, 0126, 160, 519),
            new(false, 0126, 170, 520),
            new(false, 0126, 180, 521),
            new(false, 0126, 190, 522),
            new(false, 0126, 200, 523),
            new(false, 0126, 210, 524),
            new(false, 0127, 11, 525),
            new(false, 0127, 12, 526),
            new(false, 0127, 21, 527),
            new(false, 0127, 22, 528),
            new(false, 0127, 30, 529),
            new(false, 0127, 40, 530),
            new(false, 0128, 10, 531),
            new(false, 0128, 20, 532),
            new(false, 0128, 30, 533),
            new(false, 0128, 40, 534),
            new(false, 0128, 50, 535),
            new(false, 0128, 90, 536),
            new(false, 0128, 100, 537),
            new(false, 0128, 110, 538),
            new(false, 0128, 120, 539),
            new(false, 0128, 130, 540),
            new(false, 0128, 140, 541),
            new(false, 0128, 150, 542),
            new(false, 0128, 171, 543),
            new(false, 0128, 172, 544),
            new(false, 0128, 180, 545),
            new(false, 0128, 190, 546),
            new(false, 0128, 200, 547),
            new(false, 0128, 220, 548),
            new(false, 0128, 230, 549),
            new(false, 0128, 240, 550),
            new(false, 0128, 250, 551),
            new(false, 0128, 260, 552),
            new(false, 0128, 270, 553),
            new(false, 0128, 280, 554),
            new(false, 0128, 290, 555),
            new(false, 0128, 300, 556),
            new(false, 0129, 10, 557),
            new(false, 0129, 20, 558),
            new(false, 0129, 30, 559),
            new(false, 0129, 40, 560),
            new(false, 0129, 50, 561),
            new(false, 0129, 60, 562),
            new(false, 0129, 70, 563),
            new(false, 0129, 80, 564),
            new(false, 0129, 90, 565),
            new(false, 0129, 100, 566),
            new(false, 0129, 110, 567),
            new(false, 0129, 120, 568),
            new(false, 0129, 130, 569),
            new(false, 0129, 140, 570),
            new(false, 0129, 150, 571),
            new(false, 0129, 160, 572),
            new(false, 0129, 170, 573),
            new(false, 0129, 180, 574),
            new(false, 0130, 10, 575),
            new(false, 0130, 20, 576),
            new(false, 0130, 30, 577),
            new(false, 0130, 40, 578),
            new(false, 0130, 50, 579),
            new(false, 0131, 10, 580),
            new(false, 0131, 20, 581),
            new(false, 0132, 10, 582),
            new(false, 0132, 20, 583),
            new(false, 0132, 30, 584),
            new(false, 0200, 10, 585),
            new(false, 0200, 20, 586),
            new(false, 0200, 30, 587),
            new(false, 0201, 10, 588),
            new(false, 0201, 20, 589),
            new(false, 0201, 30, 590),
            new(false, 0201, 40, 591),
            new(false, 0201, 50, 592),
            new(false, 0201, 61, 593),
            new(false, 0201, 62, 594),
            new(false, 0201, 70, 595),
            new(false, 0201, 72, 596),
            new(false, 0201, 80, 597),
            new(false, 0202, 10, 598),
            new(false, 0202, 12, 599),
            new(false, 0203, 10, 600),
            new(false, 0203, 12, 601),
            new(false, 0203, 20, 602),
            new(false, 0203, 31, 603),
            new(false, 0203, 32, 604),
            new(false, 0203, 40, 605),
            new(false, 0203, 50, 606),
            new(false, 0203, 60, 607),
            new(false, 0203, 70, 608),
            new(false, 0203, 80, 609),
            new(false, 0203, 90, 610),
            new(false, 0203, 100, 611),
            new(false, 0203, 110, 612),
            new(false, 0203, 120, 613),
            new(false, 0203, 122, 614),
            new(false, 0203, 130, 615),
            new(false, 0203, 140, 616),
            new(false, 0203, 150, 617),
            new(false, 0203, 160, 618),
            new(false, 0203, 170, 619),
            new(false, 0203, 180, 620),
            new(false, 0203, 190, 621),
            new(false, 0203, 192, 622),
            new(false, 0203, 200, 623),
            new(false, 0203, 210, 624),
            new(false, 0203, 211, 625),
            new(false, 0203, 212, 626),
            new(false, 0203, 220, 627),
            new(false, 0203, 230, 628),
            new(false, 0203, 240, 629),
            new(false, 0203, 250, 630),
            new(false, 0203, 260, 631),
            new(false, 0203, 270, 632),
            new(false, 0203, 280, 633),
            new(false, 0203, 290, 634),
            new(false, 0203, 300, 635),
            new(false, 0203, 310, 636),
            new(false, 0203, 320, 637),
            new(false, 0204, 10, 638),
            new(false, 0204, 20, 639),
            new(false, 0204, 30, 640),
            new(false, 0204, 40, 641),
            new(false, 0204, 50, 642),
            new(false, 0204, 60, 643),
            new(false, 0204, 70, 644),
            new(false, 0204, 80, 645),
            new(false, 0204, 90, 646),
            new(false, 0204, 100, 647),
            new(false, 0204, 110, 648),
            new(false, 0204, 120, 649),
            new(false, 0204, 130, 650),
            new(false, 0204, 140, 651),
            new(false, 0204, 142, 652),
            new(false, 0204, 143, 653),
            new(false, 0204, 150, 654),
            new(false, 0204, 160, 655),
            new(false, 0204, 170, 656),
            new(false, 0204, 180, 657),
            new(false, 0205, 10, 658),
            new(false, 0205, 20, 659),
            new(false, 0205, 30, 660),
            new(false, 0205, 40, 661),
            new(false, 0205, 50, 662),
            new(false, 0205, 60, 663),
            new(false, 0205, 70, 664),
            new(false, 0205, 80, 665),
            new(false, 0205, 90, 666),
            new(false, 0205, 91, 667),
            new(false, 0205, 100, 668),
            new(false, 0206, 11, 669),
            new(false, 0206, 12, 670),
            new(false, 0206, 20, 671),
            new(false, 0206, 30, 672),
            new(false, 0206, 40, 673),
            new(false, 0206, 50, 674),
            new(false, 0206, 60, 675),
            new(false, 0206, 70, 676),
            new(false, 0206, 80, 677),
            new(false, 0207, 10, 678),
            new(false, 0207, 20, 679),
            new(false, 0207, 30, 680),
            new(false, 0207, 40, 681),
            new(false, 0207, 50, 682),
            new(false, 0207, 60, 683),
            new(false, 0208, 10, 684),
            new(false, 0208, 20, 685),
            new(false, 0208, 30, 686),
            new(false, 0208, 40, 687),
            new(false, 0208, 50, 688),
            new(false, 0208, 61, 689),
            new(false, 0208, 62, 690),
            new(false, 0208, 70, 691),
            new(false, 0208, 80, 692),
            new(false, 0208, 90, 693),
            new(false, 0208, 100, 694),
            new(false, 0208, 110, 695),
            new(false, 0209, 10, 696),
            new(false, 0209, 20, 697),
            new(false, 0209, 30, 698),
            new(false, 0210, 10, 699),
            new(false, 0210, 20, 700),
            new(false, 0210, 30, 701),
            new(false, 0210, 40, 702),
            new(false, 0210, 50, 703),
            new(false, 0210, 60, 704),
            new(false, 0210, 70, 705),
            new(false, 0210, 80, 706),
            new(false, 0210, 90, 707),
            new(false, 0210, 100, 708),
            new(false, 0210, 110, 709),
            new(false, 0210, 120, 710),
            new(false, 0210, 130, 711),
            new(false, 0210, 140, 712),
            new(false, 0210, 150, 713),
            new(false, 0210, 160, 714),
            new(false, 0210, 170, 715),
            new(false, 0210, 180, 716),
            new(false, 0210, 190, 717),
            new(false, 0210, 200, 718),
            new(false, 0210, 210, 719),
        };

        public bool miniEvent;
        public int eventID;
        public int voiceID;
        public int fileID;

        public VoiceEntry(bool miniEvent, int eventID, int voiceID, int fileID)
        {
            this.miniEvent = miniEvent;
            this.eventID = eventID;
            this.voiceID = voiceID;
            this.fileID = fileID;
        }

        private class VoiceINI
        {
            public string EventID { get; set; }
            public int EventVoiceID { get; set; }
            public int VoiceFileID { get; set; }

            public VoiceINI()
            {
                EventID = string.Empty;
            }
        }

        public static VoiceEntry[] LoadVoiceArrayINI(string filepath)
        {
            VoiceINI[] voiceArray = Common.Ini.IniSerializer.DeserializeFromFile<VoiceINI[]>(filepath) 
                ?? throw new InvalidDataException("Error reading voice array INI file");

            VoiceEntry[] result = new VoiceEntry[voiceArray.Length];
            for(int i = 0; i < voiceArray.Length; i++)
            {
                VoiceINI? ini = voiceArray[i];
                if(ini == null)
                {
                    continue;
                }

                result[i] = new(ini.EventID[0] == 'M', int.Parse(ini.EventID.Substring(ini.EventID.Length - 4, 4)), ini.EventVoiceID, ini.VoiceFileID);
            }

            return result;
        }
    }
}