import { readFileSync } from 'node:fs'
import { resolve } from 'node:path'
import { Page } from '@playwright/test'

export interface PokemonFixture {
    items: Array<{ id: number | null; name: string }>
    count: number
}

const fixtureRoot = resolve(__dirname, '..', '..', 'testfixtures')

export function loadFixture(scenario: string, layer = 'frontend'): unknown {
    const path = resolve(fixtureRoot, scenario, layer, 'test-data.json')
    return JSON.parse(readFileSync(path, 'utf8')) as unknown
}

export async function stubPokemonApi(
    page: Page,
    body: unknown,
    status = 200
): Promise<void> {
    await page.route('**/api/pokemon', (route) =>
        route.fulfill({
            status,
            contentType: 'application/json',
            body: JSON.stringify(body)
        })
    )
}
