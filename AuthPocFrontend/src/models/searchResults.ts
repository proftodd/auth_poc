export type Entity = {
    id: number;
    substance?: Substance;
    num9000?: Num9000;
    descriptors: Array<Descriptor>;
}

export type Substance = {
    inchiKey: string;
    inchi: string;
}

export type Num9000 = {
    num: number;
}

export type Descriptor = {
    desc: string;
}

export type SearchResult = {
    searchTerms: Array<string>;
    entities: Array<Entity>;
}
